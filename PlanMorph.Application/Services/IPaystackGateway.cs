using PlanMorph.Application.DTOs.Payments;

namespace PlanMorph.Application.Services;

public interface IPaystackGateway
{
    Task<PaymentInitializationResult?> InitializePaymentAsync(
        string email,
        decimal amount,
        string reference,
        string? subaccountCode = null,
        int? transactionChargeKobo = null,
        string? splitCode = null);
    Task<PaymentVerificationResult?> VerifyPaymentAsync(string reference);
    Task<IReadOnlyList<PaystackBankOptionDto>> GetTransferBanksAsync(string country = "kenya", string? currency = null);
    Task<PaystackTransferRecipientResult?> CreateTransferRecipientAsync(PaystackTransferRecipientRequest request);
    Task<PaystackTransferResult?> InitiateTransferAsync(PaystackTransferRequest request);
    bool IsSignatureValid(string payload, string signature);
}
