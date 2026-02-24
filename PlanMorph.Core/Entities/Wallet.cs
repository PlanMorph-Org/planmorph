namespace PlanMorph.Core.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Currency { get; set; } = "KES";
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal PendingBalance { get; set; }
    public long RowVersion { get; set; }

    public decimal AvailableBalance => TotalEarned - TotalWithdrawn - PendingBalance;
}
