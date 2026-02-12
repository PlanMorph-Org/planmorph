namespace PlanMorph.Application.DTOs.Payments;

public class PaymentVerificationResult
{
    public bool IsSuccessful { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int AmountKobo { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
