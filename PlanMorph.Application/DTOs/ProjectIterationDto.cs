using System.ComponentModel.DataAnnotations;
using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Application.DTOs;

// ──────────────────────────────────────────
// Iteration DTOs
// ──────────────────────────────────────────

public record ProjectIterationDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public int IterationNumber { get; init; }
    public Guid SubmittedById { get; init; }
    public string SubmittedByName { get; init; } = string.Empty;
    public SubmitterRole SubmittedByRole { get; init; }
    public IterationStatus Status { get; init; }
    public string? Notes { get; init; }
    public string? ReviewNotes { get; init; }
    public Guid? ReviewedById { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<ProjectFileDto> Files { get; init; } = new();
}

public record SubmitIterationDto
{
    [StringLength(3000)]
    public string? Notes { get; init; }
}

public record ReviewIterationDto
{
    [Required]
    public bool Approved { get; init; }

    [StringLength(3000)]
    public string? ReviewNotes { get; init; }
}

// ──────────────────────────────────────────
// File DTOs
// ──────────────────────────────────────────

public record ProjectFileDto
{
    public Guid Id { get; init; }
    public Guid? IterationId { get; init; }
    public Guid ProjectId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public ProjectFileCategory Category { get; init; }
    public string StorageUrl { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public int Version { get; init; }
    public Guid UploadedById { get; init; }
    public string UploadedByName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

// ──────────────────────────────────────────
// Client Deliverable DTOs
// ──────────────────────────────────────────

public record ClientDeliverableDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid IterationId { get; init; }
    public int DeliveryNumber { get; init; }
    public Guid DeliveredByMentorId { get; init; }
    public string MentorName { get; init; } = string.Empty;
    public DeliverableStatus Status { get; init; }
    public string? ClientNotes { get; init; }
    public string? MentorNotes { get; init; }
    public DateTime DeliveredAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public List<ProjectFileDto> Files { get; init; } = new();
}

public record DeliverToClientDto
{
    [StringLength(2000)]
    public string? MentorNotes { get; init; }
}

public record ReviewDeliverableDto
{
    [Required]
    public bool Accepted { get; init; }

    [StringLength(2000)]
    public string? ClientNotes { get; init; }
}

// ──────────────────────────────────────────
// Message DTOs
// ──────────────────────────────────────────

public record ProjectMessageDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public ProjectMessageSenderRole SenderRole { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsSystemMessage { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SendMessageDto
{
    [Required]
    [StringLength(5000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}
