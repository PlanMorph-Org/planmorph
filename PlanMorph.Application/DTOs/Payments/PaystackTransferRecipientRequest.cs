namespace PlanMorph.Application.DTOs.Payments;

public class PaystackTransferRecipientRequest
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankCode { get; set; }
    public string Currency { get; set; } = "KES";
}
