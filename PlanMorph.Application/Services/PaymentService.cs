using Microsoft.AspNetCore.Identity;
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

    public PaymentService(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IPaystackGateway paystackGateway,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _paystackGateway = paystackGateway;
        _emailService = emailService;
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

        var reference = order.OrderNumber;
        var init = await _paystackGateway.InitializePaymentAsync(client.Email, order.Amount, reference);
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

        order.Status = OrderStatus.Paid;
        order.PaymentReference = reference;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
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
