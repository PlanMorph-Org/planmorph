using System.ComponentModel.DataAnnotations;
using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Application.DTOs;

public record StudentApplicationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public ApplicationType ApplicationType { get; init; }
    public Guid? InvitedByMentorId { get; init; }
    public string? InvitedByMentorName { get; init; }
    public StudentType StudentType { get; init; }
    public string UniversityName { get; init; } = string.Empty;
    public string? StudentIdNumber { get; init; }
    public string? TranscriptUrl { get; init; }
    public string? SchoolIdUrl { get; init; }
    public string? PortfolioUrl { get; init; }
    public string? CoverLetterUrl { get; init; }
    public ApplicationStatus Status { get; init; }
    public string? ReviewNotes { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateStudentApplicationDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public StudentType StudentType { get; init; }

    [Required]
    [StringLength(300, MinimumLength = 3)]
    public string UniversityName { get; init; } = string.Empty;

    [StringLength(100)]
    public string? StudentIdNumber { get; init; }

    public string? PortfolioUrl { get; init; }

    public string? PhoneNumber { get; init; }
}

public record ReviewStudentApplicationDto
{
    [StringLength(2000)]
    public string? ReviewNotes { get; init; }
}

public record StudentProfileDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public StudentType StudentType { get; init; }
    public string UniversityName { get; init; } = string.Empty;
    public EnrollmentStatus EnrollmentStatus { get; init; }
    public DateTime? ExpectedGraduation { get; init; }
    public string? StudentIdNumber { get; init; }
    public Guid? MentorId { get; init; }
    public string? MentorName { get; init; }
    public MentorshipStatus MentorshipStatus { get; init; }
    public int TotalProjectsCompleted { get; init; }
    public decimal AverageRating { get; init; }
    public decimal TotalEarnings { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UpdateStudentProfileDto
{
    [StringLength(300)]
    public string? UniversityName { get; init; }

    public EnrollmentStatus? EnrollmentStatus { get; init; }

    public DateTime? ExpectedGraduation { get; init; }
}
