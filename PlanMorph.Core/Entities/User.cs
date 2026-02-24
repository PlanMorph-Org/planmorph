using Microsoft.AspNetCore.Identity;

namespace PlanMorph.Core.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Professional details (Architect/Engineer)
    public string? ProfessionalLicense { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? Specialization { get; set; }
    public string? CvUrl { get; set; }
    public string? CoverLetterUrl { get; set; }
    public string? WorkExperienceUrl { get; set; }
    public string? CvFileName { get; set; }
    public long? CvFileSizeBytes { get; set; }
    public DateTime? CvUploadedAt { get; set; }
    public string? CoverLetterFileName { get; set; }
    public long? CoverLetterFileSizeBytes { get; set; }
    public DateTime? CoverLetterUploadedAt { get; set; }
    public string? WorkExperienceFileName { get; set; }
    public long? WorkExperienceFileSizeBytes { get; set; }
    public DateTime? WorkExperienceUploadedAt { get; set; }

    public bool LicenseVerified { get; set; }
    public bool DocumentsVerified { get; set; }
    public bool ExperienceVerified { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public Guid? LastReviewedById { get; set; }
    public string? LastReviewedByName { get; set; }
    public bool IsRejected { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedById { get; set; }

    // Marketplace payment profile
    public bool IsFoundingMember { get; set; }
    public int? FoundingMemberSlot { get; set; }
    public string? PaystackSubaccountCode { get; set; }
    public string? PaystackRecipientCode { get; set; }
    
    // Navigation properties
    public ICollection<Design> Designs { get; set; } = new List<Design>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<DesignVerification> Verifications { get; set; } = new List<DesignVerification>();
    public ICollection<ProfessionalReviewLog> ProfessionalReviewLogs { get; set; } = new List<ProfessionalReviewLog>();
    public Wallet? Wallet { get; set; }
}

public enum UserRole
{
    Client,
    Architect,
    Contractor,
    Admin,
    Engineer,
    Student
}
