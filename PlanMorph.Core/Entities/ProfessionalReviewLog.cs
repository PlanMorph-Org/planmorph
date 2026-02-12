namespace PlanMorph.Core.Entities;

public class ProfessionalReviewLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProfessionalUserId { get; set; }
    public Guid AdminUserId { get; set; }
    public ProfessionalReviewAction Action { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool LicenseVerified { get; set; }
    public bool DocumentsVerified { get; set; }
    public bool ExperienceVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? ProfessionalUser { get; set; }
    public User? AdminUser { get; set; }
}

public enum ProfessionalReviewAction
{
    Approved,
    Rejected
}
