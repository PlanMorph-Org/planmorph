using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Application.DTOs.Payments;
using PlanMorph.Application.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarningsController : ControllerBase
{
    private const decimal PlatformReserveKes = 150m;

    private readonly ApplicationDbContext _dbContext;
    private readonly IPaystackGateway _paystackGateway;
    private readonly ICommissionService _commissionService;
    private readonly ILogger<EarningsController> _logger;

    public EarningsController(
        ApplicationDbContext dbContext,
        IPaystackGateway paystackGateway,
        ICommissionService commissionService,
        ILogger<EarningsController> logger)
    {
        _dbContext = dbContext;
        _paystackGateway = paystackGateway;
        _commissionService = commissionService;
        _logger = logger;
    }

    [Authorize(Roles = "Architect,Engineer,Student")]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Invalid user identity" });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var gross = await CalculateGrossEarningsAsync(userId, role);
        var wallet = await EnsureWalletAsync(userId, gross);
        var available = wallet.AvailableBalance;
        var withdrawable = Math.Max(0m, available - PlatformReserveKes);

        var hasCashoutToday = await _dbContext.PayoutRequests
            .AnyAsync(p => p.UserId == userId && p.CreatedAt.Date == DateTime.UtcNow.Date && !p.IsDeleted);

        var latestCashoutAt = await _dbContext.PayoutRequests
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => (DateTime?)p.CreatedAt)
            .FirstOrDefaultAsync();

        if (IsStudent(role))
        {
            var taskEarnings = await GetStudentTaskEarningsAsync(userId);
            var cashouts = await GetCashoutHistoryAsync(userId);

            return Ok(new
            {
                role,
                totalEarnings = wallet.TotalEarned,
                totalSuccessfulCashouts = wallet.TotalWithdrawn,
                availableBalance = available,
                reserveAmount = PlatformReserveKes,
                withdrawableBalance = withdrawable,
                canCashoutToday = !hasCashoutToday,
                lastCashoutAt = latestCashoutAt,
                taskEarnings,
                cashouts
            });
        }

        var sales = await GetProfessionalSalesAsync(userId);
        var thisMonth = sales
            .Where(s => s.PaidAtUtc.Month == DateTime.UtcNow.Month && s.PaidAtUtc.Year == DateTime.UtcNow.Year)
            .Sum(s => s.CommissionAmount);

        return Ok(new
        {
            role,
            totalEarnings = wallet.TotalEarned,
            totalSuccessfulCashouts = wallet.TotalWithdrawn,
            availableBalance = available,
            reserveAmount = PlatformReserveKes,
            withdrawableBalance = withdrawable,
            canCashoutToday = !hasCashoutToday,
            lastCashoutAt = latestCashoutAt,
            thisMonthEarnings = thisMonth,
            totalSales = sales.Count,
            sales,
            cashouts = await GetCashoutHistoryAsync(userId)
        });
    }

    [Authorize(Roles = "Architect,Engineer,Student")]
    [HttpPost("cashout")]
    public async Task<IActionResult> RequestCashout([FromBody] CashoutRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "Invalid user identity" });

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        if (!CanCashout(role))
            return Forbid();

        if (!ValidateCashoutDestination(dto, out var destinationError))
            return BadRequest(new { message = destinationError });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero" });

        var now = DateTime.UtcNow;
        var idempotencyKey = string.IsNullOrWhiteSpace(dto.IdempotencyKey)
            ? $"PM-CO-{userId:N}-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
            : dto.IdempotencyKey.Trim();

        var existing = await _dbContext.PayoutRequests.FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey && !p.IsDeleted);
        if (existing != null)
        {
            return Ok(new
            {
                message = "Cashout request already processed",
                reference = existing.Reference,
                status = existing.Status
            });
        }

        var reference = $"PMPO-{now:yyyyMMdd}-{Guid.NewGuid():N}";

        PayoutRequest payout;
        Wallet wallet;

        await using (var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
        {
            var hasCashoutToday = await _dbContext.PayoutRequests
                .AnyAsync(p => p.UserId == userId && p.CreatedAt.Date == now.Date && !p.IsDeleted);

            if (hasCashoutToday)
                return BadRequest(new { message = "Cashout can only be requested once per day" });

            var gross = await CalculateGrossEarningsAsync(userId, role);
            wallet = await EnsureWalletAsync(userId, gross);

            var availableBefore = wallet.AvailableBalance;
            var withdrawable = Math.Max(0m, availableBefore - PlatformReserveKes);

            if (dto.Amount > withdrawable)
            {
                return BadRequest(new
                {
                    message = "Cashout amount exceeds withdrawable balance after reserve",
                    withdrawableBalance = withdrawable,
                    reserveAmount = PlatformReserveKes
                });
            }

            payout = new PayoutRequest
            {
                UserId = userId,
                Role = role,
                GrossEarningsSnapshot = gross,
                PriorSuccessfulCashoutsSnapshot = wallet.TotalWithdrawn,
                AvailableBeforeRequest = availableBefore,
                ReserveAmount = PlatformReserveKes,
                Amount = dto.Amount,
                Channel = dto.Channel,
                RecipientName = dto.RecipientName.Trim(),
                DestinationMasked = BuildDestinationMask(dto),
                Reference = reference,
                IdempotencyKey = idempotencyKey,
                LockedAt = now,
                Status = PayoutStatus.Processing,
                CreatedAt = now,
                UpdatedAt = now
            };

            var pendingBefore = wallet.PendingBalance;
            wallet.PendingBalance += dto.Amount;
            wallet.RowVersion += 1;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _dbContext.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = userId,
                TransactionType = WalletTransactionType.LockWithdrawal,
                Amount = dto.Amount,
                BalanceBefore = pendingBefore,
                BalanceAfter = wallet.PendingBalance,
                Currency = "KES",
                IdempotencyKey = idempotencyKey,
                ExternalReference = reference,
                RelatedPayoutRequestId = payout.Id,
                MetadataJson = "{\"phase\":\"lock\"}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _dbContext.PayoutRequests.AddAsync(payout);

            await _dbContext.FinancialAuditLogs.AddAsync(new FinancialAuditLog
            {
                ActorUserId = userId,
                TargetUserId = userId,
                Action = "withdrawal_locked",
                Reference = reference,
                DetailsJson = $"{{\"amount\":{dto.Amount},\"idempotencyKey\":\"{idempotencyKey}\"}}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }

        try
        {
            var recipientType = dto.Channel == PayoutChannel.Bank ? "nuban" : "mobile_money";
            var accountNumber = dto.Channel == PayoutChannel.Bank ? dto.AccountNumber!.Trim() : dto.MobileNumber!.Trim();

            var recipient = await _paystackGateway.CreateTransferRecipientAsync(new PaystackTransferRecipientRequest
            {
                Type = recipientType,
                Name = dto.RecipientName.Trim(),
                AccountNumber = accountNumber,
                BankCode = dto.BankCode?.Trim(),
                Currency = "KES"
            });

            if (recipient == null || !recipient.IsSuccessful)
            {
                await ReleaseLockedWithdrawalAsync(userId, payout, dto.Amount, reference, recipient?.Message ?? "Failed to create transfer recipient", idempotencyKey);
                return BadRequest(new { message = "Unable to process cashout recipient at this time" });
            }

            payout.RecipientCode = recipient.RecipientCode;
            payout.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var transfer = await _paystackGateway.InitiateTransferAsync(new PaystackTransferRequest
            {
                Amount = dto.Amount,
                RecipientCode = recipient.RecipientCode,
                Reference = reference,
                Reason = $"PlanMorph cashout ({role})"
            });

            if (transfer == null)
            {
                await ReleaseLockedWithdrawalAsync(userId, payout, dto.Amount, reference, "Failed to initiate transfer", idempotencyKey);
                return BadRequest(new { message = "Cashout transfer failed to initialize" });
            }

            if (!transfer.IsSuccessful && !transfer.IsPending)
            {
                await ReleaseLockedWithdrawalAsync(userId, payout, dto.Amount, reference, transfer.Message ?? "Transfer failed", idempotencyKey);
                return BadRequest(new { message = transfer.Message ?? "Cashout transfer failed" });
            }

            await using (var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var gross = await CalculateGrossEarningsAsync(userId, role);
                var currentWallet = await EnsureWalletAsync(userId, gross);

                var withdrawnBefore = currentWallet.TotalWithdrawn;
                var pendingBefore = currentWallet.PendingBalance;

                currentWallet.PendingBalance = Math.Max(0m, pendingBefore - dto.Amount);
                currentWallet.TotalWithdrawn += dto.Amount;
                currentWallet.RowVersion += 1;
                currentWallet.UpdatedAt = DateTime.UtcNow;

                payout.TransferCode = transfer.TransferCode;
                payout.ProcessedAt = DateTime.UtcNow;
                payout.UpdatedAt = DateTime.UtcNow;
                payout.Status = PayoutStatus.Completed;
                payout.FailureReason = null;

                await _dbContext.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = currentWallet.Id,
                    UserId = userId,
                    TransactionType = WalletTransactionType.DebitWithdrawn,
                    Amount = dto.Amount,
                    BalanceBefore = withdrawnBefore,
                    BalanceAfter = currentWallet.TotalWithdrawn,
                    Currency = "KES",
                    IdempotencyKey = idempotencyKey + "-debit",
                    ExternalReference = reference,
                    RelatedPayoutRequestId = payout.Id,
                    MetadataJson = $"{{\"pendingBefore\":{pendingBefore},\"pendingAfter\":{currentWallet.PendingBalance}}}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _dbContext.FinancialAuditLogs.AddAsync(new FinancialAuditLog
                {
                    ActorUserId = userId,
                    TargetUserId = userId,
                    Action = "withdrawal_completed",
                    Reference = reference,
                    DetailsJson = $"{{\"amount\":{dto.Amount},\"transferCode\":\"{transfer.TransferCode}\"}}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _dbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }


            return Ok(new
            {
                message = transfer.IsPending ? "Cashout initiated and is pending confirmation" : "Cashout processed successfully",
                reference,
                transferCode = transfer.TransferCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cashout failed for user {UserId}, reference {Reference}", userId, reference);

            await ReleaseLockedWithdrawalAsync(userId, payout, dto.Amount, reference, "Unexpected processing error", idempotencyKey);

            return StatusCode(500, new { message = "Unexpected error processing cashout" });
        }
    }

    [Authorize(Roles = "Architect,Engineer,Student")]
    [HttpGet("payout-options")]
    public async Task<IActionResult> GetPayoutOptions([FromQuery] PayoutChannel? channel = null)
    {
        var options = await _paystackGateway.GetTransferBanksAsync("kenya", "KES");

        var filtered = options.AsEnumerable();
        if (channel == PayoutChannel.MobileMoney)
        {
            var mobile = filtered.Where(o =>
                (!string.IsNullOrWhiteSpace(o.Type) && o.Type.Contains("mobile", StringComparison.OrdinalIgnoreCase))
                || o.Name.Contains("M-PESA", StringComparison.OrdinalIgnoreCase)
                || o.Name.Contains("Airtel", StringComparison.OrdinalIgnoreCase)
                || o.Name.Contains("Telkom", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            filtered = mobile.Count > 0 ? mobile : filtered;
        }

        return Ok(filtered.Select(o => new { name = o.Name, code = o.Code, type = o.Type }));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("commission/quote")]
    public async Task<IActionResult> QuoteCommission([FromBody] CommissionQuoteRequestDto dto)
    {
        if (dto.AmountKes <= 0)
            return BadRequest(new { message = "Amount must be greater than zero" });

        if (dto.RevenueType == CommissionRevenueType.ContractReferral)
        {
            var quote = await _commissionService.GetContractReferralCommissionAsync(dto.AmountKes);
            return Ok(quote);
        }

        if (dto.ProfessionalUserId == null)
            return BadRequest(new { message = "ProfessionalUserId is required for design sale quotes" });

        var professional = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.ProfessionalUserId.Value);
        if (professional == null)
            return NotFound(new { message = "Professional user not found" });

        var designQuote = await _commissionService.GetDesignSaleCommissionAsync(dto.AmountKes, professional);
        return Ok(designQuote);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private static bool CanCashout(string role)
    {
        return role is "Architect" or "Engineer" or "Student";
    }

    private static bool IsStudent(string role)
    {
        return string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<decimal> CalculateGrossEarningsAsync(Guid userId, string role)
    {
        if (IsStudent(role))
        {
            return await _dbContext.MentorshipProjects
                .Where(p => p.StudentId == userId
                    && !p.IsDeleted
                    && (p.PaymentStatus == Core.Entities.Mentorship.PaymentStatus.StudentReleased
                        || p.PaymentStatus == Core.Entities.Mentorship.PaymentStatus.Completed))
                .SumAsync(p => (decimal?)p.StudentFee) ?? 0m;
        }

        return await _dbContext.Orders
            .Include(o => o.Design)
            .Where(o => !o.IsDeleted
                && (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Completed)
                && !o.Design.IsDeleted
                && o.Design.ArchitectId == userId)
            .SumAsync(o => (decimal?)(o.ProfessionalNetAmount > 0 ? o.ProfessionalNetAmount : (o.Amount * 0.70m))) ?? 0m;
    }

    private async Task<List<ProfessionalSaleDto>> GetProfessionalSalesAsync(Guid userId)
    {
        var sales = await _dbContext.Orders
            .Include(o => o.Design)
            .Where(o => !o.IsDeleted
                && (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Completed)
                && !o.Design.IsDeleted
                && o.Design.ArchitectId == userId)
            .OrderByDescending(o => o.PaidAt ?? o.CreatedAt)
            .Select(o => new ProfessionalSaleDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                DesignTitle = o.Design.Title,
                SaleAmount = o.Amount,
                CommissionAmount = o.ProfessionalNetAmount > 0 ? o.ProfessionalNetAmount : (o.Amount * 0.70m),
                PaidAtUtc = o.PaidAt ?? o.CreatedAt
            })
            .ToListAsync();

        return sales;
    }

    private async Task<List<StudentTaskEarningDto>> GetStudentTaskEarningsAsync(Guid userId)
    {
        return await _dbContext.MentorshipProjects
            .Where(p => p.StudentId == userId
                && !p.IsDeleted
                && (p.PaymentStatus == Core.Entities.Mentorship.PaymentStatus.StudentReleased
                    || p.PaymentStatus == Core.Entities.Mentorship.PaymentStatus.Completed))
            .OrderByDescending(p => p.PaidAt ?? p.CompletedAt ?? p.CreatedAt)
            .Select(p => new StudentTaskEarningDto
            {
                ProjectId = p.Id,
                ProjectNumber = p.ProjectNumber,
                TaskTitle = p.Title,
                TaskCategory = p.Category.ToString(),
                OrganisationSetFee = p.StudentFee,
                EarnedAtUtc = p.PaidAt ?? p.CompletedAt ?? p.CreatedAt,
                PaymentStatus = p.PaymentStatus.ToString()
            })
            .ToListAsync();
    }

    private async Task<decimal> GetSuccessfulCashoutsAsync(Guid userId)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
        return wallet?.TotalWithdrawn ?? 0m;
    }

    private async Task<Wallet> EnsureWalletAsync(Guid userId, decimal totalEarnedBaseline)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Currency = "KES",
                TotalEarned = totalEarnedBaseline,
                TotalWithdrawn = 0,
                PendingBalance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _dbContext.Wallets.AddAsync(wallet);
            await _dbContext.SaveChangesAsync();
            return wallet;
        }

        if (wallet.TotalEarned < totalEarnedBaseline)
        {
            wallet.TotalEarned = totalEarnedBaseline;
            wallet.RowVersion += 1;
            wallet.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        return wallet;
    }

    private async Task ReleaseLockedWithdrawalAsync(
        Guid userId,
        PayoutRequest payout,
        decimal amount,
        string reference,
        string reason,
        string idempotencyKey)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var gross = await CalculateGrossEarningsAsync(userId, payout.Role);
        var wallet = await EnsureWalletAsync(userId, gross);

        var pendingBefore = wallet.PendingBalance;
        wallet.PendingBalance = Math.Max(0m, pendingBefore - amount);
        wallet.RowVersion += 1;
        wallet.UpdatedAt = DateTime.UtcNow;

        payout.Status = PayoutStatus.Failed;
        payout.FailureReason = reason;
        payout.ProcessedAt = DateTime.UtcNow;
        payout.UpdatedAt = DateTime.UtcNow;

        await _dbContext.WalletTransactions.AddAsync(new WalletTransaction
        {
            WalletId = wallet.Id,
            UserId = userId,
            TransactionType = WalletTransactionType.UnlockWithdrawal,
            Amount = amount,
            BalanceBefore = pendingBefore,
            BalanceAfter = wallet.PendingBalance,
            Currency = "KES",
            IdempotencyKey = idempotencyKey + "-unlock",
            ExternalReference = reference,
            RelatedPayoutRequestId = payout.Id,
            MetadataJson = $"{{\"reason\":\"{reason.Replace("\"", "'")}\"}}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.FinancialAuditLogs.AddAsync(new FinancialAuditLog
        {
            ActorUserId = userId,
            TargetUserId = userId,
            Action = "withdrawal_failed_unlocked",
            Reference = reference,
            DetailsJson = $"{{\"amount\":{amount},\"reason\":\"{reason.Replace("\"", "'")}\"}}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<List<CashoutHistoryItemDto>> GetCashoutHistoryAsync(Guid userId)
    {
        return await _dbContext.PayoutRequests
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new CashoutHistoryItemDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Channel = p.Channel,
                DestinationMasked = p.DestinationMasked,
                Status = p.Status,
                Reference = p.Reference,
                CreatedAtUtc = p.CreatedAt,
                ProcessedAtUtc = p.ProcessedAt,
                FailureReason = p.FailureReason
            })
            .ToListAsync();
    }

    private static bool ValidateCashoutDestination(CashoutRequestDto dto, out string message)
    {
        if (string.IsNullOrWhiteSpace(dto.RecipientName) || dto.RecipientName.Trim().Length < 3)
        {
            message = "Recipient name is required";
            return false;
        }

        if (dto.Channel == PayoutChannel.Bank)
        {
            if (string.IsNullOrWhiteSpace(dto.AccountNumber) || dto.AccountNumber.Trim().Length < 6)
            {
                message = "Valid bank account number is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.BankCode))
            {
                message = "Bank code is required for bank cashout";
                return false;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.MobileNumber) || dto.MobileNumber.Trim().Length < 8)
            {
                message = "Valid mobile number is required for mobile cashout";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.BankCode))
            {
                message = "Provider code is required for mobile cashout";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private static string BuildDestinationMask(CashoutRequestDto dto)
    {
        var raw = dto.Channel == PayoutChannel.Bank ? dto.AccountNumber ?? string.Empty : dto.MobileNumber ?? string.Empty;
        var trimmed = raw.Trim();
        if (trimmed.Length <= 4)
            return "****";

        var tail = trimmed[^4..];
        return $"****{tail}";
    }

    public class CashoutRequestDto
    {
        [Range(typeof(decimal), "1", "100000000")]
        public decimal Amount { get; set; }

        [Required]
        public PayoutChannel Channel { get; set; }

        [Required]
        [MaxLength(200)]
        public string RecipientName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(50)]
        public string? MobileNumber { get; set; }

        [MaxLength(20)]
        public string? BankCode { get; set; }

        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    public class CommissionQuoteRequestDto
    {
        [Range(typeof(decimal), "1", "1000000000")]
        public decimal AmountKes { get; set; }
        public CommissionRevenueType RevenueType { get; set; }
        public Guid? ProfessionalUserId { get; set; }
    }

    public class ProfessionalSaleDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string DesignTitle { get; set; } = string.Empty;
        public decimal SaleAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime PaidAtUtc { get; set; }
    }

    public class StudentTaskEarningDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectNumber { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskCategory { get; set; } = string.Empty;
        public decimal OrganisationSetFee { get; set; }
        public DateTime EarnedAtUtc { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class CashoutHistoryItemDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public PayoutChannel Channel { get; set; }
        public string DestinationMasked { get; set; } = string.Empty;
        public PayoutStatus Status { get; set; }
        public string Reference { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ProcessedAtUtc { get; set; }
        public string? FailureReason { get; set; }
    }
}
