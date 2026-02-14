using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ClientDeliverable : BaseEntity
{
    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public Guid IterationId { get; set; }
    public ProjectIteration Iteration { get; set; } = null!;

    public int DeliveryNumber { get; set; }

    public Guid DeliveredByMentorId { get; set; }
    public User DeliveredByMentor { get; set; } = null!;

    public DeliverableStatus Status { get; set; } = DeliverableStatus.Delivered;

    public string? ClientNotes { get; set; }
    public string? MentorNotes { get; set; }

    public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}

public enum DeliverableStatus
{
    Delivered,
    UnderClientReview,
    Accepted,
    RevisionRequested
}
