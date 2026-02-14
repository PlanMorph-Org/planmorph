namespace PlanMorph.Core.Entities;

public class Ticket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty; // Changed from Title
    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketCategory Category { get; set; } = TicketCategory.General;

    // Relationships
    public Guid ClientId { get; set; } // Changed back to Guid to match User.Id
    public User? Client { get; set; }

    public Guid? AssignedToAdminId { get; set; } // Changed from AssignedToId for clarity
    public User? AssignedTo { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? DesignId { get; set; }
    public Design? Design { get; set; }

    public DateTime? ClosedAt { get; set; } // Added for tracking closure time

    // Navigation
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}

public enum TicketStatus
{
    Open,
    Assigned,
    InProgress,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TicketCategory
{
    Technical,
    Billing,
    Design,
    Order,
    Construction,
    General
}
