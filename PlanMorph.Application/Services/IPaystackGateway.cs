using PlanMorph.Application.DTOs.Payments;

namespace PlanMorph.Application.Services;

public interface IPaystackGateway
{
    Task<PaymentInitializationResult?> InitializePaymentAsync(string email, decimal amount, string reference);
    Task<PaymentVerificationResult?> VerifyPaymentAsync(string reference);
    bool IsSignatureValid(string payload, string signature);
}
