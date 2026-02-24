namespace PlanMorph.Core.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public WalletTransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Currency { get; set; } = "KES";

    public string? IdempotencyKey { get; set; }
    public string? ExternalReference { get; set; }
    public Guid? RelatedPayoutRequestId { get; set; }
    public string? MetadataJson { get; set; }
}

public enum WalletTransactionType
{
    CreditEarned,
    DebitWithdrawn,
    LockWithdrawal,
    UnlockWithdrawal,
    PlatformCommission,
    Adjustment
}
