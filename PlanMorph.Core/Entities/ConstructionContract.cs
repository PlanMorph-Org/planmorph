namespace PlanMorph.Core.Entities;

public class ConstructionContract : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid? ContractorId { get; set; }
    public User? Contractor { get; set; }
    
    public string Location { get; set; } = string.Empty; // City/Region (Kenya only for now)
    public decimal EstimatedCost { get; set; }
    public decimal CommissionAmount { get; set; } // 2% commission
    public ContractStatus Status { get; set; } = ContractStatus.Pending;
    
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public enum ContractStatus
{
    Pending,
    Assigned,
    InProgress,
    Completed,
    Cancelled
}
