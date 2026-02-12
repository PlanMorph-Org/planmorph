namespace PlanMorph.Application.DTOs.Payments;

public class PaymentInitializationResult
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
}
