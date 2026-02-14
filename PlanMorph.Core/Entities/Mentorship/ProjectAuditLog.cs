using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ProjectAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public Guid ActorId { get; set; }
    public User Actor { get; set; } = null!;
    public string ActorRole { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Metadata { get; set; } // JSON

    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
