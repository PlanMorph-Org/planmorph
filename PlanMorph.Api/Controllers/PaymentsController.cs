using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs.Payments;
using PlanMorph.Application.Services;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPaystackGateway _paystackGateway;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, IPaystackGateway paystackGateway, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _paystackGateway = paystackGateway;
        _logger = logger;
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

        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.TryGetProperty("event", out var eventProperty))
            eventType = eventProperty.GetString();

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
            var verified = await _paymentService.VerifyPaystackAsync(reference);
            if (!verified)
            {
                _logger.LogWarning("Paystack webhook verification failed for reference {Reference}", reference);
            }
        }

        return Ok();
    }
}
