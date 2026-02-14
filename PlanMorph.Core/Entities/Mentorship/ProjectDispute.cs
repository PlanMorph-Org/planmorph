using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Entities.Mentorship;

public class ProjectDispute : BaseEntity
{
    public Guid ProjectId { get; set; }
    public MentorshipProject Project { get; set; } = null!;

    public Guid RaisedById { get; set; }
    public User RaisedBy { get; set; } = null!;
    public string RaisedByRole { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DisputeStatus Status { get; set; } = DisputeStatus.Open;

    public string? Resolution { get; set; }
    public Guid? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum DisputeStatus
{
    Open,
    UnderInvestigation,
    Resolved,
    Dismissed
}
