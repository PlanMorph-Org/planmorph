namespace PlanMorph.Core.Entities;

public class DesignVerification : BaseEntity
{
    public Guid DesignId { get; set; }
    public Design Design { get; set; } = null!;

    public Guid? VerifierUserId { get; set; }
    public User? VerifierUser { get; set; }

    public VerificationType VerificationType { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    public string? Comments { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public enum VerificationType
{
    Architectural,      // Only architects can verify
    Structural,         // Only engineers can verify
    BOQ,                // Legacy: either architect or engineer can verify
    BOQArchitect,       // BOQ verified by architect
    BOQEngineer         // BOQ verified by engineer
}

public enum VerificationStatus
{
    Pending,
    Verified,
    Rejected
}
