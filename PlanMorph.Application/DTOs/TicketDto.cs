using PlanMorph.Core.Entities;

namespace PlanMorph.Application.DTOs;

public record TicketDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; } // Changed to Guid
    public string Subject { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TicketStatus Status { get; init; }
    public TicketPriority Priority { get; init; }
    public TicketCategory Category { get; init; }
    public Guid? AssignedToAdminId { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? DesignId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public List<TicketMessageDto> Messages { get; init; } = new();
    public int UnreadMessageCount { get; init; }
}