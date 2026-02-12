namespace PlanMorph.Core.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;
    
    public Guid DesignId { get; set; }
    public Design Design { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public bool IncludesConstruction { get; set; } = false;
    public Guid? ContractorId { get; set; }
    public User? Contractor { get; set; }
    
    // Navigation
    public ConstructionContract? ConstructionContract { get; set; }
}

public enum OrderStatus
{
    Pending,
    Paid,
    Completed,
    Cancelled,
    Refunded
}

public enum PaymentMethod
{
    MPesa,
    Card,
    BankTransfer,
    Paystack
}
