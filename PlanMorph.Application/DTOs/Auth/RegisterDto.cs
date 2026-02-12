namespace PlanMorph.Application.DTOs.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Role { get; set; } // Optional: "Architect" or null for Client

    // Professional details (optional for clients)
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
}
