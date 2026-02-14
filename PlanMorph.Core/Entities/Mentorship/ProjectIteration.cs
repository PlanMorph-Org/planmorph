using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ProjectIteration : BaseEntity
{
    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public int IterationNumber { get; set; }

    public Guid SubmittedById { get; set; }
    public User SubmittedBy { get; set; } = null!;
    public SubmitterRole SubmittedByRole { get; set; }

    public IterationStatus Status { get; set; } = IterationStatus.Submitted;

    public string? Notes { get; set; }
    public string? ReviewNotes { get; set; }

    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation properties
    public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();
}

public enum SubmitterRole
{
    Student,
    Mentor
}

public enum IterationStatus
{
    Submitted,
    UnderReview,
    Approved,
    RevisionRequested,
    Superseded
}
