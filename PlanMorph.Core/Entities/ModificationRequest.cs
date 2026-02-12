namespace PlanMorph.Core.Entities;

public class ModificationRequest : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid DesignId { get; set; }
    public Design Design { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    public decimal? QuotedPrice { get; set; }
    public ModificationStatus Status { get; set; } = ModificationStatus.Pending;
    
    public DateTime? CompletedAt { get; set; }
}

public enum ModificationStatus
{
    Pending,
    Quoted,
    Accepted,
    InProgress,
    Completed,
    Rejected
}