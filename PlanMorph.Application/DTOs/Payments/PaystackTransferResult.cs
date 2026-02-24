namespace PlanMorph.Application.DTOs.Payments;

public class PaystackTransferResult
{
    public bool IsSuccessful { get; set; }
    public bool IsPending { get; set; }
    public string? TransferCode { get; set; }
    public string? Message { get; set; }
}
