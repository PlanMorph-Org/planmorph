namespace PlanMorph.Application.DTOs.Design;

public class DesignDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double SquareFootage { get; set; }
    public int Stories { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal EstimatedConstructionCost { get; set; }
    public List<string> PreviewImages { get; set; } = new();
    public List<string> PreviewVideos { get; set; } = new();
}