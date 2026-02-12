namespace PlanMorph.Application.DTOs.Modification;

public class ModificationRequestDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid DesignId { get; set; }
    public string DesignTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? QuotedPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}