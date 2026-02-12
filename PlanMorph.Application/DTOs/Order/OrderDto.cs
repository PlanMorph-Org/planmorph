namespace PlanMorph.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid DesignId { get; set; }
    public string DesignTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IncludesConstruction { get; set; }
    public ConstructionContractDto? ConstructionContract { get; set; }
}

public class ConstructionContractDto
{
    public Guid Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ContractorName { get; set; }
}