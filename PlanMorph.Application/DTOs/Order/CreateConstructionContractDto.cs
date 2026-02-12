namespace PlanMorph.Application.DTOs.Order;

public class CreateConstructionContractDto
{
    public string Location { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public Guid? ContractorId { get; set; } // Optional - admin can assign later
}