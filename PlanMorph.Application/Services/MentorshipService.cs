using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Application.Services;

public class MentorshipService : IMentorshipService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectWorkflowService _workflowService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<MentorshipService> _logger;

    public MentorshipService(
        IUnitOfWork unitOfWork,
        IProjectWorkflowService workflowService,
        UserManager<User> userManager,
        ILogger<MentorshipService> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _userManager = userManager;
        _logger = logger;
    }

    // ──────────────────────────────────────────
    // Client operations
    // ──────────────────────────────────────────

    public async Task<MentorshipProject> CreateProjectAsync(
        Guid clientId, string title, string description,
        string? requirements, ProjectType projectType, DesignCategory category,
        ProjectPriority priority, Guid? orderId, Guid? designId)
    {
        var projectNumber = await GenerateProjectNumberAsync();

        var project = new MentorshipProject
        {
            ProjectNumber = projectNumber,
            ClientId = clientId,
            Title = title,
            Description = description,
            Requirements = requirements,
            ProjectType = projectType,
            Category = category,
            Priority = priority,
            Status = ProjectStatus.Draft,
            OrderId = orderId,
            DesignId = designId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MentorshipProjects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Transition to Submitted
        await _workflowService.TransitionAsync(project.Id, ProjectStatus.Submitted, clientId, "Client submitted project request");

        _logger.LogInformation("Mentorship project {ProjectNumber} created by client {ClientId}", projectNumber, clientId);

        return project;
    }

    public async Task<IEnumerable<MentorshipProject>> GetClientProjectsAsync(Guid clientId)
    {
        return await _unitOfWork.MentorshipProjectRepository.GetProjectsByClientIdAsync(clientId);
    }

    public async Task<MentorshipProject?> GetProjectByIdAsync(Guid projectId)
    {
        return await _unitOfWork.MentorshipProjectRepository.GetProjectWithDetailsAsync(projectId);
    }

    public async Task<bool> CancelProjectAsync(Guid projectId, Guid clientId, string? reason)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.ClientId != clientId)
            return false;

        // Client can only cancel before StudentAssigned
        var cancellableStates = new HashSet<ProjectStatus>
        {
            ProjectStatus.Draft, ProjectStatus.Submitted,
            ProjectStatus.UnderReview, ProjectStatus.Scoped,
            ProjectStatus.Published
        };

        if (!cancellableStates.Contains(project.Status))
            return false;

        return await _workflowService.TransitionAsync(projectId, ProjectStatus.Cancelled, clientId, reason);
    }

    // ──────────────────────────────────────────
    // Admin operations
    // ──────────────────────────────────────────

    public async Task<IEnumerable<MentorshipProject>> GetAllProjectsAsync()
    {
        return await _unitOfWork.MentorshipProjects.GetAllAsync();
    }

    public async Task<IEnumerable<MentorshipProject>> GetProjectsByStatusAsync(ProjectStatus status)
    {
        return await _unitOfWork.MentorshipProjectRepository.GetProjectsByStatusAsync(status);
    }

    public async Task<bool> ScopeProjectAsync(
        Guid projectId, Guid adminId, string scope,
        decimal clientFee, decimal mentorFee, decimal studentFee,
        int estimatedDays, int maxRevisions)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Must be Submitted or UnderReview to scope
        if (project.Status != ProjectStatus.Submitted && project.Status != ProjectStatus.UnderReview)
            return false;

        // If still Submitted, transition to UnderReview first
        if (project.Status == ProjectStatus.Submitted)
        {
            var moved = await _workflowService.TransitionAsync(projectId, ProjectStatus.UnderReview, adminId);
            if (!moved) return false;
        }

        // Calculate platform fee (15% of client fee)
        var platformFee = clientFee * 0.15m;

        project.Scope = scope;
        project.ClientFee = clientFee;
        project.MentorFee = mentorFee;
        project.StudentFee = studentFee;
        project.PlatformFee = platformFee;
        project.EstimatedDeliveryDays = estimatedDays;
        project.MaxRevisions = maxRevisions;
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.MentorshipProjects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        // Transition to Scoped
        return await _workflowService.TransitionAsync(projectId, ProjectStatus.Scoped, adminId, "Admin scoped project");
    }

    public async Task<bool> PublishProjectAsync(Guid projectId, Guid adminId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.Status != ProjectStatus.Scoped)
            return false;

        return await _workflowService.TransitionAsync(projectId, ProjectStatus.Published, adminId, "Admin published project");
    }

    public async Task<bool> AssignMentorAsync(Guid projectId, Guid adminId, Guid mentorId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.Status != ProjectStatus.Published)
            return false;

        var mentor = await _userManager.FindByIdAsync(mentorId.ToString());
        if (mentor == null || !mentor.IsActive)
            return false;

        if (mentor.Role != UserRole.Architect && mentor.Role != UserRole.Engineer)
            return false;

        // Check mentor has a profile
        var mentorProfile = await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == mentorId);
        if (mentorProfile == null)
            return false;

        project.MentorId = mentorId;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return await _workflowService.TransitionAsync(projectId, ProjectStatus.Claimed, adminId, $"Admin assigned mentor {mentorId}");
    }

    // ──────────────────────────────────────────
    // Mentor operations
    // ──────────────────────────────────────────

    public async Task<IEnumerable<MentorshipProject>> GetPublishedProjectsAsync()
    {
        return await _unitOfWork.MentorshipProjectRepository.GetPublishedProjectsAsync();
    }

    public async Task<bool> ClaimProjectAsync(Guid projectId, Guid mentorId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.Status != ProjectStatus.Published)
            return false;

        // Check mentor profile and capacity
        var mentorProfile = await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == mentorId);
        if (mentorProfile == null)
            throw new InvalidOperationException("You must activate your mentor profile first.");

        var activeCount = await _unitOfWork.MentorshipProjectRepository
            .GetActiveProjectCountByMentorAsync(mentorId);
        if (activeCount >= mentorProfile.MaxConcurrentProjects)
            throw new InvalidOperationException(
                $"You have reached your maximum of {mentorProfile.MaxConcurrentProjects} concurrent projects.");

        project.MentorId = mentorId;
        project.MentorDeadline = DateTime.UtcNow.AddDays(project.EstimatedDeliveryDays);
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return await _workflowService.TransitionAsync(projectId, ProjectStatus.Claimed, mentorId, "Mentor claimed project");
    }

    public async Task<IEnumerable<MentorshipProject>> GetMentorProjectsAsync(Guid mentorId)
    {
        return await _unitOfWork.MentorshipProjectRepository.GetProjectsByMentorIdAsync(mentorId);
    }

    public async Task<bool> AssignStudentAsync(Guid projectId, Guid mentorId, Guid studentId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.MentorId != mentorId || project.Status != ProjectStatus.Claimed)
            return false;

        // Verify student is one of mentor's students
        var relationship = await _unitOfWork.MentorStudentRelationships
            .FirstOrDefaultAsync(r => r.MentorId == mentorId && r.StudentId == studentId
                && r.Status == RelationshipStatus.Active);
        if (relationship == null)
            throw new InvalidOperationException("This student is not in your active roster.");

        // Check student capacity
        var studentActiveCount = await _unitOfWork.MentorshipProjectRepository
            .GetActiveProjectCountByStudentAsync(studentId);
        if (studentActiveCount >= 3) // students max 3 concurrent projects
            throw new InvalidOperationException("This student already has 3 active projects.");

        project.StudentId = studentId;
        project.StudentDeadline = DateTime.UtcNow.AddDays(Math.Max(project.EstimatedDeliveryDays - 3, 7));
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        var transitioned = await _workflowService.TransitionAsync(
            projectId, ProjectStatus.StudentAssigned, mentorId, $"Student {studentId} assigned");

        if (transitioned)
        {
            // Auto-transition to InProgress
            await _workflowService.TransitionAsync(
                projectId, ProjectStatus.InProgress, mentorId, "Student assignment triggers InProgress");
        }

        return transitioned;
    }

    // ──────────────────────────────────────────
    // Shared
    // ──────────────────────────────────────────

    public async Task<string> GenerateProjectNumberAsync()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _unitOfWork.MentorshipProjects.CountAsync() + 1;
        return $"MP-{date}-{count:D4}";
    }

    // ──────────────────────────────────────────
    // Iteration operations
    // ──────────────────────────────────────────

    public async Task<ProjectIteration> SubmitIterationAsync(Guid projectId, Guid studentId, string? notes)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException("Project not found.");

        if (project.StudentId != studentId)
            throw new InvalidOperationException("You are not assigned to this project.");

        if (project.Status != ProjectStatus.InProgress)
            throw new InvalidOperationException("Project is not in a state that accepts submissions.");

        // Mark any previous submitted iterations as Superseded
        var previousIterations = await _unitOfWork.ProjectIterations
            .FindAsync(i => i.ProjectId == projectId && i.Status == IterationStatus.Submitted);
        foreach (var prev in previousIterations)
        {
            prev.Status = IterationStatus.Superseded;
            prev.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProjectIterations.UpdateAsync(prev);
        }

        // Get next iteration number
        var iterationCount = await _unitOfWork.ProjectIterations
            .CountAsync(i => i.ProjectId == projectId);

        var iteration = new ProjectIteration
        {
            ProjectId = projectId,
            IterationNumber = iterationCount + 1,
            SubmittedById = studentId,
            SubmittedByRole = SubmitterRole.Student,
            Status = IterationStatus.Submitted,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProjectIterations.AddAsync(iteration);
        await _unitOfWork.SaveChangesAsync();

        // Transition project to UnderMentorReview
        await _workflowService.TransitionAsync(projectId, ProjectStatus.UnderMentorReview, studentId, $"Iteration {iteration.IterationNumber} submitted");

        // Add audit log
        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = projectId,
            ActorId = studentId,
            Action = "IterationSubmitted",
            NewValue = $"Iteration #{iteration.IterationNumber}",
            Metadata = notes,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Student {StudentId} submitted iteration {Number} for project {ProjectId}",
            studentId, iteration.IterationNumber, projectId);

        return iteration;
    }

    public async Task<IEnumerable<ProjectIteration>> GetIterationsAsync(Guid projectId)
    {
        return await _unitOfWork.ProjectIterations
            .FindWithIncludesAsync(i => i.ProjectId == projectId, i => i.Files);
    }

    public async Task<ProjectIteration?> GetIterationByIdAsync(Guid iterationId)
    {
        var iterations = await _unitOfWork.ProjectIterations
            .FindWithIncludesAsync(i => i.Id == iterationId, i => i.Files);
        return iterations.FirstOrDefault();
    }

    public async Task<bool> ReviewIterationAsync(Guid iterationId, Guid mentorId, bool approved, string? reviewNotes)
    {
        var iteration = await _unitOfWork.ProjectIterations.GetByIdAsync(iterationId);
        if (iteration == null)
            return false;

        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(iteration.ProjectId);
        if (project == null || project.MentorId != mentorId)
            return false;

        if (iteration.Status != IterationStatus.Submitted)
            return false;

        iteration.Status = approved ? IterationStatus.Approved : IterationStatus.RevisionRequested;
        iteration.ReviewNotes = reviewNotes;
        iteration.ReviewedById = mentorId;
        iteration.ReviewedAt = DateTime.UtcNow;
        iteration.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProjectIterations.UpdateAsync(iteration);
        await _unitOfWork.SaveChangesAsync();

        // Transition project status
        if (approved)
        {
            await _workflowService.TransitionAsync(iteration.ProjectId, ProjectStatus.MentorApproved, mentorId,
                $"Iteration {iteration.IterationNumber} approved");
        }
        else
        {
            await _workflowService.TransitionAsync(iteration.ProjectId, ProjectStatus.RevisionRequested, mentorId,
                reviewNotes ?? $"Iteration {iteration.IterationNumber} revision requested");
        }

        _logger.LogInformation("Mentor {MentorId} {Action} iteration {IterationId}",
            mentorId, approved ? "approved" : "requested revision on", iterationId);

        return true;
    }

    // ──────────────────────────────────────────
    // Client deliverable operations
    // ──────────────────────────────────────────

    public async Task<ClientDeliverable> DeliverToClientAsync(Guid projectId, Guid mentorId, Guid iterationId, string? mentorNotes)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.MentorId != mentorId)
            throw new InvalidOperationException("Project not found or you are not the mentor.");

        if (project.Status != ProjectStatus.MentorApproved)
            throw new InvalidOperationException("Project must be in MentorApproved state to deliver to client.");

        var iteration = await _unitOfWork.ProjectIterations.GetByIdAsync(iterationId);
        if (iteration == null || iteration.ProjectId != projectId || iteration.Status != IterationStatus.Approved)
            throw new InvalidOperationException("Invalid or unapproved iteration.");

        var deliveryCount = await _unitOfWork.ClientDeliverables
            .CountAsync(d => d.ProjectId == projectId);

        var deliverable = new ClientDeliverable
        {
            ProjectId = projectId,
            IterationId = iterationId,
            DeliveryNumber = deliveryCount + 1,
            DeliveredByMentorId = mentorId,
            Status = DeliverableStatus.Delivered,
            MentorNotes = mentorNotes,
            DeliveredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ClientDeliverables.AddAsync(deliverable);
        await _unitOfWork.SaveChangesAsync();

        // Transition to ClientReview
        await _workflowService.TransitionAsync(projectId, ProjectStatus.ClientReview, mentorId,
            $"Delivery #{deliverable.DeliveryNumber} sent to client");

        _logger.LogInformation("Mentor {MentorId} delivered iteration {IterationId} to client for project {ProjectId}",
            mentorId, iterationId, projectId);

        return deliverable;
    }

    public async Task<IEnumerable<ClientDeliverable>> GetDeliverablesAsync(Guid projectId)
    {
        return await _unitOfWork.ClientDeliverables
            .FindAsync(d => d.ProjectId == projectId);
    }

    public async Task<bool> AcceptDeliverableAsync(Guid deliverableId, Guid clientId, string? clientNotes)
    {
        var deliverable = await _unitOfWork.ClientDeliverables.GetByIdAsync(deliverableId);
        if (deliverable == null)
            return false;

        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(deliverable.ProjectId);
        if (project == null || project.ClientId != clientId)
            return false;

        if (deliverable.Status != DeliverableStatus.Delivered)
            return false;

        deliverable.Status = DeliverableStatus.Accepted;
        deliverable.ClientNotes = clientNotes;
        deliverable.ReviewedAt = DateTime.UtcNow;
        deliverable.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ClientDeliverables.UpdateAsync(deliverable);
        await _unitOfWork.SaveChangesAsync();

        // Transition project to Completed
        await _workflowService.TransitionAsync(deliverable.ProjectId, ProjectStatus.Completed, clientId,
            "Client accepted deliverable");

        _logger.LogInformation("Client {ClientId} accepted deliverable {DeliverableId}", clientId, deliverableId);

        return true;
    }

    public async Task<bool> RequestDeliverableRevisionAsync(Guid deliverableId, Guid clientId, string? clientNotes)
    {
        var deliverable = await _unitOfWork.ClientDeliverables.GetByIdAsync(deliverableId);
        if (deliverable == null)
            return false;

        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(deliverable.ProjectId);
        if (project == null || project.ClientId != clientId)
            return false;

        if (deliverable.Status != DeliverableStatus.Delivered)
            return false;

        // Check revision limit
        if (project.CurrentRevisionCount >= project.MaxRevisions)
            throw new InvalidOperationException($"Maximum revision count ({project.MaxRevisions}) has been reached.");

        deliverable.Status = DeliverableStatus.RevisionRequested;
        deliverable.ClientNotes = clientNotes;
        deliverable.ReviewedAt = DateTime.UtcNow;
        deliverable.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ClientDeliverables.UpdateAsync(deliverable);
        await _unitOfWork.SaveChangesAsync();

        // Transition project to ClientRevisionRequested → then back to InProgress
        await _workflowService.TransitionAsync(deliverable.ProjectId, ProjectStatus.ClientRevisionRequested, clientId,
            clientNotes ?? "Client requested revision");

        _logger.LogInformation("Client {ClientId} requested revision on deliverable {DeliverableId}", clientId, deliverableId);

        return true;
    }

    // ──────────────────────────────────────────
    // Messaging
    // ──────────────────────────────────────────

    public async Task<ProjectMessage> SendMessageAsync(Guid projectId, Guid senderId, ProjectMessageSenderRole role, string content)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            throw new InvalidOperationException("Project not found.");

        // Validate sender is part of the project
        if (role == ProjectMessageSenderRole.Student && project.StudentId != senderId)
            throw new InvalidOperationException("You are not assigned to this project.");
        if (role == ProjectMessageSenderRole.Mentor && project.MentorId != senderId)
            throw new InvalidOperationException("You are not the mentor for this project.");

        var message = new ProjectMessage
        {
            ProjectId = projectId,
            SenderId = senderId,
            SenderRole = role,
            Content = content,
            IsSystemMessage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProjectMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        return message;
    }

    public async Task<IEnumerable<ProjectMessage>> GetMessagesAsync(Guid projectId)
    {
        return await _unitOfWork.ProjectMessages
            .FindAsync(m => m.ProjectId == projectId);
    }

    // ──────────────────────────────────────────
    // Student project queries
    // ──────────────────────────────────────────

    public async Task<IEnumerable<MentorshipProject>> GetStudentProjectsAsync(Guid studentId)
    {
        return await _unitOfWork.MentorshipProjectRepository.GetProjectsByStudentIdAsync(studentId);
    }
}
