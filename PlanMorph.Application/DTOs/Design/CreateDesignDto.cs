using PlanMorph.Core.Entities;

namespace PlanMorph.Application.DTOs.Design;

public class CreateDesignDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double SquareFootage { get; set; }
    public int Stories { get; set; }
    public DesignCategory Category { get; set; }
    public decimal EstimatedConstructionCost { get; set; }
}