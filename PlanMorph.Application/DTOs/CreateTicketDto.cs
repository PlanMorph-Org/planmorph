using PlanMorph.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace PlanMorph.Application.DTOs;

public record CreateTicketDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Subject { get; init; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public TicketCategory Category { get; init; }

    [Required]
    public TicketPriority Priority { get; init; }

    public Guid? OrderId { get; init; }
    public Guid? DesignId { get; init; }
}