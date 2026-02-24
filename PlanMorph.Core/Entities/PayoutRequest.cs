namespace PlanMorph.Core.Entities;

public class PayoutRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Role { get; set; } = string.Empty;

    public decimal GrossEarningsSnapshot { get; set; }
    public decimal PriorSuccessfulCashoutsSnapshot { get; set; }
    public decimal AvailableBeforeRequest { get; set; }
    public decimal ReserveAmount { get; set; } = 150m;
    public decimal Amount { get; set; }

    public PayoutChannel Channel { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string DestinationMasked { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public string? RecipientCode { get; set; }
    public string? TransferCode { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Processing;
}

public enum PayoutChannel
{
    Bank,
    MobileMoney
}

public enum PayoutStatus
{
    Processing,
    Completed,
    Failed
}
