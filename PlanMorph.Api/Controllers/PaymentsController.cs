using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Application.DTOs.Payments;
using PlanMorph.Application.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Services;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IMentorshipPaymentService _mentorshipPaymentService;
    private readonly IPaystackGateway _paystackGateway;
    private readonly ICommissionService _commissionService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IMentorshipPaymentService mentorshipPaymentService,
        IPaystackGateway paystackGateway,
        ICommissionService commissionService,
        ApplicationDbContext dbContext,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _mentorshipPaymentService = mentorshipPaymentService;
        _paystackGateway = paystackGateway;
        _commissionService = commissionService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("referral/quote")]
    public async Task<IActionResult> QuoteReferralCommission([FromBody] ReferralCommissionQuoteDto dto)
    {
        if (dto.BqEstimatedContractValueKes <= 0)
            return BadRequest(new { message = "Contract value must be greater than zero" });

        var quote = await _commissionService.GetContractReferralCommissionAsync(dto.BqEstimatedContractValueKes);
        return Ok(quote);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("referral/initialize")]
    public async Task<IActionResult> InitializeReferralCommissionPayment([FromBody] InitializeReferralPaymentDto dto)
    {
        if (dto.BqEstimatedContractValueKes <= 0)
            return BadRequest(new { message = "Contract value must be greater than zero" });

        var contractor = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.ContractorUserId && u.Role == UserRole.Contractor && u.IsActive);
        if (contractor?.Email == null)
            return BadRequest(new { message = "Active contractor with email is required" });

        var quote = await _commissionService.GetContractReferralCommissionAsync(dto.BqEstimatedContractValueKes);
        var reference = string.IsNullOrWhiteSpace(dto.Reference)
            ? $"PMRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"
            : dto.Reference.Trim();

        var init = await _paystackGateway.InitializePaymentAsync(
            contractor.Email,
            quote.CommissionAmount,
            reference,
            null,
            null,
            null);

        if (init == null)
            return BadRequest(new { message = "Unable to initialize referral commission payment" });

        _dbContext.FinancialAuditLogs.Add(new FinancialAuditLog
        {
            ActorUserId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId) ? actorId : null,
            TargetUserId = contractor.Id,
            Action = "referral_commission_initialize",
            Reference = reference,
            DetailsJson = $"{{\"bqEstimatedContractValueKes\":{dto.BqEstimatedContractValueKes},\"ratePercent\":{quote.RatePercent},\"commissionAmount\":{quote.CommissionAmount}}}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            quote,
            payment = init
        });
    }

    [Authorize(Roles = "Client")]
    [HttpPost("paystack/initialize")]
    public async Task<IActionResult> InitializePaystack([FromBody] InitializePaystackDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paymentService.InitializePaystackAsync(dto.OrderId, userId);

        if (result == null)
            return BadRequest(new { message = "Unable to initialize payment." });

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("paystack/verify")]
    public async Task<IActionResult> VerifyPaystack([FromQuery] string reference)
    {
        var verified = await _paymentService.VerifyPaystackAsync(reference);
        if (!verified)
            return BadRequest(new { message = "Payment verification failed." });

        return Ok(new { message = "Payment verified successfully." });
    }

    [AllowAnonymous]
    [HttpPost("paystack/webhook")]
    public async Task<IActionResult> PaystackWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(payload))
            return Ok();

        var signature = Request.Headers["x-paystack-signature"].FirstOrDefault() ?? string.Empty;
        if (!_paystackGateway.IsSignatureValid(payload, signature))
            return Unauthorized();

        string? eventType = null;
        string? reference = null;
        string? eventId = null;

        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("event", out var eventProperty))
            eventType = eventProperty.GetString();

        if (document.RootElement.TryGetProperty("data", out var eventData) &&
            eventData.TryGetProperty("id", out var idProperty))
        {
            eventId = idProperty.ToString();
        }

        if (!string.IsNullOrWhiteSpace(eventId))
        {
            var duplicate = await _dbContext.PaystackEventLogs.AnyAsync(e => e.EventId == eventId && !e.IsDeleted);
            if (duplicate)
                return Ok();
        }

        var eventLog = new PaystackEventLog
        {
            EventId = eventId,
            EventType = eventType ?? "unknown",
            EventSignature = signature,
            PayloadJson = payload,
            Status = PaystackEventStatus.Received,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.PaystackEventLogs.AddAsync(eventLog);
        await _dbContext.SaveChangesAsync();

        if (string.Equals(eventType, "charge.success", StringComparison.OrdinalIgnoreCase))
        {
            if (document.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("reference", out var referenceProperty))
            {
                reference = referenceProperty.GetString();
            }
        }

        if (!string.IsNullOrWhiteSpace(reference))
        {
            // Try mentorship payment first (references start with "MP-")
            if (reference.StartsWith("MP-", StringComparison.OrdinalIgnoreCase))
            {
                var mentorshipVerified = await _mentorshipPaymentService.VerifyEscrowPaymentAsync(reference);
                if (!mentorshipVerified)
                {
                    _logger.LogWarning("Mentorship payment webhook verification failed for reference {Reference}", reference);
                    eventLog.Status = PaystackEventStatus.Failed;
                    eventLog.ErrorMessage = "Mentorship webhook verification failed";
                }
            }
            else
            {
                var verified = await _paymentService.VerifyPaystackAsync(reference);
                if (!verified)
                {
                    _logger.LogWarning("Paystack webhook verification failed for reference {Reference}", reference);
                    eventLog.Status = PaystackEventStatus.Failed;
                    eventLog.ErrorMessage = "Order webhook verification failed";
                }
            }
        }

        if (eventLog.Status != PaystackEventStatus.Failed)
            eventLog.Status = PaystackEventStatus.Processed;

        eventLog.ProcessedAt = DateTime.UtcNow;
        eventLog.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}

public class ReferralCommissionQuoteDto
{
    public decimal BqEstimatedContractValueKes { get; set; }
}

public class InitializeReferralPaymentDto
{
    public Guid ContractorUserId { get; set; }
    public decimal BqEstimatedContractValueKes { get; set; }
    public string? Reference { get; set; }
}
