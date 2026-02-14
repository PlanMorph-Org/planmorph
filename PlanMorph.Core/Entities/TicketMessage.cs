namespace PlanMorph.Core.Entities;

public class TicketMessage : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid AuthorId { get; set; } // Changed to Guid to match User.Id
    public string AuthorName { get; set; } = string.Empty; // Added for display
    public User? Sender { get; set; }

    public string Content { get; set; } = string.Empty; // Changed from Message
    public bool IsFromAdmin { get; set; } = false; // Changed from IsStaffMessage for clarity
    public bool IsReadByClient { get; set; } = false; // Added for tracking read status
}
