namespace PlanMorph.Application.DTOs;

public record TicketMessageDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public Guid AuthorId { get; init; } // Changed to Guid
    public string AuthorName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsFromAdmin { get; init; }
    public bool IsReadByClient { get; init; }
    public DateTime CreatedAt { get; init; }
}