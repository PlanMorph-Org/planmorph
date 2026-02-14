using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class StudentApplication : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ApplicationType ApplicationType { get; set; }

    public Guid? InvitedByMentorId { get; set; }
    public User? InvitedByMentor { get; set; }

    public StudentType StudentType { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public string? StudentIdNumber { get; set; }
    public string? SchoolIdUrl { get; set; }
    public string? TranscriptUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? CoverLetterUrl { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public enum ApplicationType
{
    SelfApply,
    MentorInvite
}

public enum ApplicationStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected
}
