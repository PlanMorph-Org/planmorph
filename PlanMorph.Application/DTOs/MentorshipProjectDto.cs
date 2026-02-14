using System.ComponentModel.DataAnnotations;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Application.DTOs;

// ──────────────────────────────────────────
// Client-facing DTOs
// ──────────────────────────────────────────

public record MentorshipProjectDto
{
    public Guid Id { get; init; }
    public string ProjectNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Requirements { get; init; }
    public ProjectType ProjectType { get; init; }
    public DesignCategory Category { get; init; }
    public ProjectStatus Status { get; init; }
    public string? Scope { get; init; }
    public int EstimatedDeliveryDays { get; init; }
    public decimal ClientFee { get; init; }
    public ProjectPriority Priority { get; init; }
    public int MaxRevisions { get; init; }
    public int CurrentRevisionCount { get; init; }
    public DateTime? MentorDeadline { get; init; }
    public DateTime? StudentDeadline { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? MentorName { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateProjectRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 20)]
    public string Description { get; init; } = string.Empty;

    [StringLength(3000)]
    public string? Requirements { get; init; }

    [Required]
    public ProjectType ProjectType { get; init; }

    [Required]
    public DesignCategory Category { get; init; }

    public ProjectPriority Priority { get; init; } = ProjectPriority.Medium;

    public Guid? OrderId { get; init; }
    public Guid? DesignId { get; init; }
}

// ──────────────────────────────────────────
// Admin DTOs
// ──────────────────────────────────────────

public record AdminProjectDto
{
    public Guid Id { get; init; }
    public string ProjectNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Requirements { get; init; }
    public ProjectType ProjectType { get; init; }
    public DesignCategory Category { get; init; }
    public ProjectStatus Status { get; init; }
    public string? Scope { get; init; }
    public int EstimatedDeliveryDays { get; init; }
    public decimal ClientFee { get; init; }
    public decimal MentorFee { get; init; }
    public decimal StudentFee { get; init; }
    public decimal PlatformFee { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public ProjectPriority Priority { get; init; }
    public int MaxRevisions { get; init; }
    public int CurrentRevisionCount { get; init; }

    // People
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public Guid? MentorId { get; init; }
    public string? MentorName { get; init; }
    public Guid? StudentId { get; init; }
    public string? StudentName { get; init; }

    // Dates
    public DateTime? MentorDeadline { get; init; }
    public DateTime? StudentDeadline { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ScopeProjectDto
{
    [Required]
    [StringLength(3000)]
    public string Scope { get; init; } = string.Empty;

    [Required]
    [Range(0.01, 10000000)]
    public decimal ClientFee { get; init; }

    [Required]
    [Range(0.01, 10000000)]
    public decimal MentorFee { get; init; }

    [Required]
    [Range(0.01, 10000000)]
    public decimal StudentFee { get; init; }

    [Range(1, 365)]
    public int EstimatedDeliveryDays { get; init; } = 14;

    [Range(1, 20)]
    public int MaxRevisions { get; init; } = 3;
}

public record AssignMentorDto
{
    [Required]
    public Guid MentorId { get; init; }
}

// ──────────────────────────────────────────
// Mentor project DTOs
// ──────────────────────────────────────────

public record MentorProjectDto
{
    public Guid Id { get; init; }
    public string ProjectNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Requirements { get; init; }
    public ProjectType ProjectType { get; init; }
    public DesignCategory Category { get; init; }
    public ProjectStatus Status { get; init; }
    public string? Scope { get; init; }
    public int EstimatedDeliveryDays { get; init; }
    public decimal ClientFee { get; init; }
    public decimal MentorFee { get; init; }
    public decimal StudentFee { get; init; }
    public ProjectPriority Priority { get; init; }
    public int MaxRevisions { get; init; }
    public int CurrentRevisionCount { get; init; }

    public string ClientName { get; init; } = string.Empty;
    public Guid? StudentId { get; init; }
    public string? StudentName { get; init; }

    public DateTime? MentorDeadline { get; init; }
    public DateTime? StudentDeadline { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AssignStudentDto
{
    [Required]
    public Guid StudentId { get; init; }
}

// ──────────────────────────────────────────
// Stats DTOs
// ──────────────────────────────────────────

public record MentorshipStatsDto
{
    public int TotalProjects { get; init; }
    public int ActiveProjects { get; init; }
    public int CompletedProjects { get; init; }
    public int PendingReviewProjects { get; init; }
    public int TotalStudents { get; init; }
    public int TotalMentors { get; init; }
    public decimal TotalRevenue { get; init; }
}
