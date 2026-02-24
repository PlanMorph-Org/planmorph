namespace PlanMorph.Application.DTOs.Payments;

public class PaystackTransferRequest
{
    public decimal Amount { get; set; }
    public string RecipientCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
