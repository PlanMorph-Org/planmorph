using PlanMorph.Core.Entities;

namespace PlanMorph.Application.DTOs;

public record TicketStatsDto
{
    public Dictionary<TicketStatus, int> StatusStats { get; init; } = new();
    public Dictionary<TicketPriority, int> PriorityStats { get; init; } = new();
    public Dictionary<TicketCategory, int> CategoryStats { get; init; } = new();
    public int TotalTickets { get; init; }
    public int OpenTickets { get; init; }
    public int ClosedTickets { get; init; }
}