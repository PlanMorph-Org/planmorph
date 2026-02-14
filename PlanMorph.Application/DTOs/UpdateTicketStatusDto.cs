using PlanMorph.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace PlanMorph.Application.DTOs;

public record UpdateTicketStatusDto
{
    [Required]
    public TicketStatus Status { get; init; }
}