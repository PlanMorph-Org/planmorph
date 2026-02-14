using System.ComponentModel.DataAnnotations;

namespace PlanMorph.Application.DTOs;

public record AddMessageDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}