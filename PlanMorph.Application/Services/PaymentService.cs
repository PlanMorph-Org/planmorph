using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PlanMorph.Application.DTOs.Payments;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IPaystackGateway _paystackGateway;
    private readonly IEmailService _emailService;
    private readonly ICommissionService _commissionService;
    private readonly IConfiguration _configuration;

    public PaymentService(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IPaystackGateway paystackGateway,
        IEmailService emailService,
        ICommissionService commissionService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _paystackGateway = paystackGateway;
        _emailService = emailService;
        _commissionService = commissionService;
        _configuration = configuration;
    }

    public async Task<PaymentInitializationResult?> InitializePaystackAsync(Guid orderId, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null || order.ClientId != userId)
            return null;

        if (order.PaymentMethod != PaymentMethod.Paystack)
            return null;

        if (order.Status != OrderStatus.Pending)
            return null;

        var client = await _userManager.FindByIdAsync(userId.ToString());
        if (client?.Email == null)
            return null;

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return null;

        var professional = await _userManager.FindByIdAsync(design.ArchitectId.ToString());
        if (professional == null)
            return null;

        var commission = await _commissionService.GetDesignSaleCommissionAsync(order.Amount, professional);
        var splitCode = _configuration["Paystack:CompanyCommissionSplitCode"];

        order.CommissionRatePercent = commission.RatePercent;
        order.CommissionAmount = commission.CommissionAmount;
        order.ProfessionalNetAmount = commission.NetAmount;
        order.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var reference = order.OrderNumber;
        var transactionChargeKobo = Convert.ToInt32(Math.Round(commission.CommissionAmount * 100m, MidpointRounding.AwayFromZero));

        var init = await _paystackGateway.InitializePaymentAsync(
            client.Email,
            order.Amount,
            reference,
            professional.PaystackSubaccountCode,
            transactionChargeKobo > 0 ? transactionChargeKobo : null,
            splitCode);
        return init;
    }

    public async Task<bool> VerifyPaystackAsync(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return false;

        var verify = await _paystackGateway.VerifyPaymentAsync(reference);
        if (verify == null || !verify.IsSuccessful)
            return false;

        var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.OrderNumber == reference);
        if (order == null)
            return false;

        if (order.PaymentMethod != PaymentMethod.Paystack)
            return false;

        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Completed)
            return true;

        var expectedAmount = Convert.ToInt32(Math.Round(order.Amount * 100m, MidpointRounding.AwayFromZero));
        if (verify.AmountKobo != expectedAmount)
            return false;

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return false;

        var professional = await _userManager.FindByIdAsync(design.ArchitectId.ToString());
        if (professional == null)
            return false;

        if (order.ProfessionalNetAmount <= 0)
        {
            var quote = await _commissionService.GetDesignSaleCommissionAsync(order.Amount, professional);
            order.CommissionRatePercent = quote.RatePercent;
            order.CommissionAmount = quote.CommissionAmount;
            order.ProfessionalNetAmount = quote.NetAmount;
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.Status = OrderStatus.Paid;
            order.PaymentReference = reference;
            order.PaidAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Orders.UpdateAsync(order);

            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == professional.Id);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = professional.Id,
                    Currency = "KES",
                    TotalEarned = 0,
                    TotalWithdrawn = 0,
                    PendingBalance = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Wallets.AddAsync(wallet);
                await _unitOfWork.SaveChangesAsync();
            }

            var before = wallet.TotalEarned;
            wallet.TotalEarned += order.ProfessionalNetAmount;
            wallet.UpdatedAt = DateTime.UtcNow;
            wallet.RowVersion += 1;
            await _unitOfWork.Wallets.UpdateAsync(wallet);

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = professional.Id,
                TransactionType = WalletTransactionType.CreditEarned,
                Amount = order.ProfessionalNetAmount,
                BalanceBefore = before,
                BalanceAfter = wallet.TotalEarned,
                Currency = "KES",
                ExternalReference = order.OrderNumber,
                MetadataJson = $"{{\"orderId\":\"{order.Id}\",\"commissionRate\":{order.CommissionRatePercent},\"commissionAmount\":{order.CommissionAmount}}}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.FinancialAuditLogs.AddAsync(new FinancialAuditLog
            {
                Action = "design_payment_verified",
                TargetUserId = professional.Id,
                Reference = reference,
                DetailsJson = $"{{\"orderId\":\"{order.Id}\",\"gross\":{order.Amount},\"net\":{order.ProfessionalNetAmount}}}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        var client = await _userManager.FindByIdAsync(order.ClientId.ToString());
        if (client?.Email != null && design != null)
        {
            await _emailService.SendOrderConfirmationEmailAsync(
                client.Email,
                $"{client.FirstName} {client.LastName}",
                design.Title,
                order.Amount
            );
        }

        return true;
    }
}
