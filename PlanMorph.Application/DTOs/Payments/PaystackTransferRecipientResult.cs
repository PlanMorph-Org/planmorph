namespace PlanMorph.Application.DTOs.Payments;

public class PaystackTransferRecipientResult
{
    public bool IsSuccessful { get; set; }
    public string RecipientCode { get; set; } = string.Empty;
    public string? Message { get; set; }
}
