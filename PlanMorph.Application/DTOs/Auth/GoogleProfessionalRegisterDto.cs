namespace PlanMorph.Application.DTOs.Auth;

public class GoogleProfessionalRegisterDto
{
    public string GoogleIdToken { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string ProfessionalLicense { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
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
