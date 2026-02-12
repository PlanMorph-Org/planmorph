using PlanMorph.Core.Entities;

namespace PlanMorph.Application.DTOs.Design;

public class DesignFilterDto
{
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public int? MaxBathrooms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DesignCategory? Category { get; set; }
    public int? Stories { get; set; }
    public double? MinSquareFootage { get; set; }
    public double? MaxSquareFootage { get; set; }
}