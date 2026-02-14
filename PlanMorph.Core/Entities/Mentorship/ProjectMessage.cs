using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ProjectMessage : BaseEntity
{
    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public ProjectMessageSenderRole SenderRole { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsSystemMessage { get; set; } = false;
}

public enum ProjectMessageSenderRole
{
    Mentor,
    Student,
    Admin
}
