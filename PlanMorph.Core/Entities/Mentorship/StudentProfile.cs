using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class StudentProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public StudentType StudentType { get; set; }
    public string UniversityName { get; set; } = string.Empty;
    public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime? ExpectedGraduation { get; set; }
    public string? StudentIdNumber { get; set; }
    public string? TranscriptUrl { get; set; }

    public Guid? MentorId { get; set; }
    public User? Mentor { get; set; }
    public MentorshipStatus MentorshipStatus { get; set; } = MentorshipStatus.Unmatched;

    public int TotalProjectsCompleted { get; set; }
    public decimal AverageRating { get; set; }
    public decimal TotalEarnings { get; set; }
}

public enum StudentType
{
    Architecture,
    Engineering
}

public enum EnrollmentStatus
{
    Enrolled,
    Graduated,
    Intern
}

public enum MentorshipStatus
{
    Unmatched,
    Matched,
    Active,
    Suspended
}
