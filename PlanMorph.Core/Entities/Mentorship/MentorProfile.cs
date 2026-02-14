using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class MentorProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int MaxConcurrentStudents { get; set; } = 3;
    public int MaxConcurrentProjects { get; set; } = 5;
    public string Bio { get; set; } = string.Empty;
    public string Specializations { get; set; } = string.Empty; // JSON array
    public bool AcceptingStudents { get; set; } = true;

    public int TotalProjectsCompleted { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalStudentsMentored { get; set; }
}
