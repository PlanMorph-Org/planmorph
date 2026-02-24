namespace PlanMorph.Core.Entities;

public class FinancialAuditLog : BaseEntity
{
    public Guid? ActorUserId { get; set; }
    public Guid? TargetUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? DetailsJson { get; set; }
}
