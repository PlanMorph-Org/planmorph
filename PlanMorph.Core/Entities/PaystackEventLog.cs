namespace PlanMorph.Core.Entities;

public class PaystackEventLog : BaseEntity
{
    public string? EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? EventSignature { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public PaystackEventStatus Status { get; set; } = PaystackEventStatus.Received;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public enum PaystackEventStatus
{
    Received,
    Processed,
    Ignored,
    Failed
}
