using PlanMorph.Application.DTOs.Payments;

namespace PlanMorph.Application.Services;

public interface IPaymentService
{
    Task<PaymentInitializationResult?> InitializePaystackAsync(Guid orderId, Guid userId);
    Task<bool> VerifyPaystackAsync(string reference);
}
