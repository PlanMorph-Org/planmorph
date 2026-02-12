using PlanMorph.Core.Entities;

namespace PlanMorph.Application.DTOs.Order;

public class CreateOrderDto
{
    public Guid DesignId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IncludesConstruction { get; set; } = false;
    public string? ConstructionLocation { get; set; } // Required if IncludesConstruction is true
    public string? ConstructionCountry { get; set; } // Required if IncludesConstruction is true (Kenya only for now)
}
