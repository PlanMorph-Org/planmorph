using System.ComponentModel.DataAnnotations;

namespace PlanMorph.Application.DTOs;

public record MentorProfileDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Specialization { get; init; }
    public int MaxConcurrentStudents { get; init; }
    public int MaxConcurrentProjects { get; init; }
    public string Bio { get; init; } = string.Empty;
    public string Specializations { get; init; } = string.Empty;
    public bool AcceptingStudents { get; init; }
    public int TotalProjectsCompleted { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalStudentsMentored { get; init; }
    public int ActiveStudentCount { get; init; }
    public int ActiveProjectCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ActivateMentorProfileDto
{
    [StringLength(2000)]
    public string? Bio { get; init; }

    [StringLength(1000)]
    public string? Specializations { get; init; }

    [Range(1, 10)]
    public int MaxConcurrentStudents { get; init; } = 3;

    [Range(1, 20)]
    public int MaxConcurrentProjects { get; init; } = 5;
}

public record UpdateMentorProfileDto
{
    [StringLength(2000)]
    public string? Bio { get; init; }

    [StringLength(1000)]
    public string? Specializations { get; init; }

    [Range(1, 10)]
    public int? MaxConcurrentStudents { get; init; }

    [Range(1, 20)]
    public int? MaxConcurrentProjects { get; init; }

    public bool? AcceptingStudents { get; init; }
}

public record InviteStudentDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;
}

public record MentorStudentDto
{
    public Guid RelationshipId { get; init; }
    public Guid StudentUserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string StudentType { get; init; } = string.Empty;
    public string UniversityName { get; init; } = string.Empty;
    public string RelationshipStatus { get; init; } = string.Empty;
    public int ProjectsCompleted { get; init; }
    public decimal? StudentRating { get; init; }
    public DateTime? StartedAt { get; init; }
}

public record MentorStatsDto
{
    public int TotalStudents { get; init; }
    public int ActiveStudents { get; init; }
    public int TotalProjectsCompleted { get; init; }
    public int ActiveProjects { get; init; }
    public decimal AverageRating { get; init; }
}
