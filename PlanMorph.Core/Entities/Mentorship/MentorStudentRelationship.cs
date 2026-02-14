using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class MentorStudentRelationship : BaseEntity
{
    public Guid MentorId { get; set; }
    public User Mentor { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public RelationshipStatus Status { get; set; } = RelationshipStatus.Invited;

    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }

    public decimal? MentorRating { get; set; }
    public decimal? StudentRating { get; set; }
    public int ProjectsCompleted { get; set; }
}

public enum RelationshipStatus
{
    Invited,
    Active,
    Paused,
    Ended
}
