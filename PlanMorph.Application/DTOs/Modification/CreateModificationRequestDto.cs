namespace PlanMorph.Application.DTOs.Modification;

public class CreateModificationRequestDto
{
    public Guid OrderId { get; set; }
    public string Description { get; set; } = string.Empty;
}