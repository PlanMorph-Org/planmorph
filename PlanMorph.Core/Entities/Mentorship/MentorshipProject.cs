using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class MentorshipProject : BaseEntity
{
    public string ProjectNumber { get; set; } = string.Empty;

    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;

    public Guid? MentorId { get; set; }
    public User? Mentor { get; set; }

    public Guid? StudentId { get; set; }
    public User? Student { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Requirements { get; set; }

    public ProjectType ProjectType { get; set; }
    public DesignCategory Category { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public string? Scope { get; set; }
    public int EstimatedDeliveryDays { get; set; }

    public decimal ClientFee { get; set; }
    public decimal MentorFee { get; set; }
    public decimal StudentFee { get; set; }
    public decimal PlatformFee { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? DesignId { get; set; }
    public Design? Design { get; set; }

    public int MaxRevisions { get; set; } = 3;
    public int CurrentRevisionCount { get; set; }

    public DateTime? MentorDeadline { get; set; }
    public DateTime? StudentDeadline { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation properties
    public ICollection<ProjectIteration> Iterations { get; set; } = new List<ProjectIteration>();
    public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
    public ICollection<ProjectMessage> Messages { get; set; } = new List<ProjectMessage>();
    public ICollection<ClientDeliverable> Deliverables { get; set; } = new List<ClientDeliverable>();
    public ProjectDispute? Dispute { get; set; }
    public ICollection<ProjectAuditLog> AuditLogs { get; set; } = new List<ProjectAuditLog>();
}

public enum ProjectType
{
    CustomCommission,
    DesignModification
}

public enum ProjectStatus
{
    Draft,
    Submitted,
    UnderReview,
    Scoped,
    Published,
    Claimed,
    StudentAssigned,
    InProgress,
    UnderMentorReview,
    RevisionRequested,
    MentorApproved,
    ClientReview,
    ClientRevisionRequested,
    Completed,
    Paid,
    Disputed,
    Cancelled
}

public enum ProjectPriority
{
    Low,
    Medium,
    High
}

public enum PaymentStatus
{
    Pending,
    Escrowed,
    MentorReleased,
    StudentReleased,
    Completed,
    Disputed,
    Refunded
}
