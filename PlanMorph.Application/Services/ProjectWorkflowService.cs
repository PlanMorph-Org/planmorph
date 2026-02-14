using Microsoft.Extensions.Logging;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Application.Services;

public class ProjectWorkflowService : IProjectWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectWorkflowService> _logger;

    private static readonly Dictionary<ProjectStatus, HashSet<ProjectStatus>> AllowedTransitions = new()
    {
        [ProjectStatus.Draft] = new() { ProjectStatus.Submitted, ProjectStatus.Cancelled },
        [ProjectStatus.Submitted] = new() { ProjectStatus.UnderReview, ProjectStatus.Cancelled },
        [ProjectStatus.UnderReview] = new() { ProjectStatus.Scoped, ProjectStatus.Cancelled },
        [ProjectStatus.Scoped] = new() { ProjectStatus.Published, ProjectStatus.Cancelled },
        [ProjectStatus.Published] = new() { ProjectStatus.Claimed, ProjectStatus.Cancelled },
        [ProjectStatus.Claimed] = new() { ProjectStatus.StudentAssigned, ProjectStatus.Published, ProjectStatus.Cancelled },
        [ProjectStatus.StudentAssigned] = new() { ProjectStatus.InProgress, ProjectStatus.Cancelled },
        [ProjectStatus.InProgress] = new() { ProjectStatus.UnderMentorReview, ProjectStatus.Disputed, ProjectStatus.Cancelled },
        [ProjectStatus.UnderMentorReview] = new() { ProjectStatus.MentorApproved, ProjectStatus.RevisionRequested, ProjectStatus.Disputed },
        [ProjectStatus.RevisionRequested] = new() { ProjectStatus.InProgress },
        [ProjectStatus.MentorApproved] = new() { ProjectStatus.ClientReview },
        [ProjectStatus.ClientReview] = new() { ProjectStatus.Completed, ProjectStatus.ClientRevisionRequested, ProjectStatus.Disputed },
        [ProjectStatus.ClientRevisionRequested] = new() { ProjectStatus.InProgress },
        [ProjectStatus.Completed] = new() { ProjectStatus.Paid },
        [ProjectStatus.Paid] = new(),
        [ProjectStatus.Disputed] = new() { ProjectStatus.InProgress, ProjectStatus.Cancelled },
        [ProjectStatus.Cancelled] = new()
    };

    public ProjectWorkflowService(IUnitOfWork unitOfWork, ILogger<ProjectWorkflowService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public IEnumerable<ProjectStatus> GetAllowedTransitions(ProjectStatus currentStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var transitions)
            ? transitions
            : Enumerable.Empty<ProjectStatus>();
    }

    public bool IsValidTransition(ProjectStatus from, ProjectStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var transitions) && transitions.Contains(to);
    }

    public async Task<bool> TransitionAsync(Guid projectId, ProjectStatus newStatus, Guid actorId, string? notes = null)
    {
        var project = await _unitOfWork.MentorshipProjectRepository.GetProjectWithDetailsAsync(projectId);
        if (project == null)
            return false;

        var oldStatus = project.Status;

        if (!IsValidTransition(oldStatus, newStatus))
        {
            _logger.LogWarning(
                "Invalid state transition attempted: {Old} → {New} on project {ProjectId} by {ActorId}",
                oldStatus, newStatus, projectId, actorId);
            return false;
        }

        project.Status = newStatus;
        project.UpdatedAt = DateTime.UtcNow;

        // Handle status-specific side effects
        switch (newStatus)
        {
            case ProjectStatus.Completed:
                project.CompletedAt = DateTime.UtcNow;
                break;
            case ProjectStatus.Paid:
                project.PaidAt = DateTime.UtcNow;
                break;
            case ProjectStatus.Cancelled:
                project.CancelledAt = DateTime.UtcNow;
                project.CancellationReason = notes;
                break;
            case ProjectStatus.RevisionRequested:
            case ProjectStatus.ClientRevisionRequested:
                project.CurrentRevisionCount++;
                break;
        }

        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Write audit log
        var auditLog = new ProjectAuditLog
        {
            ProjectId = projectId,
            ActorId = actorId,
            Action = "StatusChanged",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString(),
            Metadata = notes,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.ProjectAuditLogs.AddAsync(auditLog);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Project {ProjectId} transitioned {Old} → {New} by {ActorId}",
            projectId, oldStatus, newStatus, actorId);

        return true;
    }
}
