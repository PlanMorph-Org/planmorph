using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Services;

public interface IMentorshipService
{
    // Client operations
    Task<MentorshipProject> CreateProjectAsync(Guid clientId, string title, string description,
        string? requirements, ProjectType projectType, DesignCategory category,
        ProjectPriority priority, Guid? orderId, Guid? designId);
    Task<IEnumerable<MentorshipProject>> GetClientProjectsAsync(Guid clientId);
    Task<MentorshipProject?> GetProjectByIdAsync(Guid projectId);
    Task<bool> CancelProjectAsync(Guid projectId, Guid clientId, string? reason);

    // Admin operations
    Task<IEnumerable<MentorshipProject>> GetAllProjectsAsync();
    Task<IEnumerable<MentorshipProject>> GetProjectsByStatusAsync(ProjectStatus status);
    Task<bool> ScopeProjectAsync(Guid projectId, Guid adminId, string scope,
        decimal clientFee, decimal mentorFee, decimal studentFee,
        int estimatedDays, int maxRevisions);
    Task<bool> PublishProjectAsync(Guid projectId, Guid adminId);
    Task<bool> AssignMentorAsync(Guid projectId, Guid adminId, Guid mentorId);

    // Mentor operations
    Task<IEnumerable<MentorshipProject>> GetPublishedProjectsAsync();
    Task<bool> ClaimProjectAsync(Guid projectId, Guid mentorId);
    Task<IEnumerable<MentorshipProject>> GetMentorProjectsAsync(Guid mentorId);
    Task<bool> AssignStudentAsync(Guid projectId, Guid mentorId, Guid studentId);

    // Shared
    Task<string> GenerateProjectNumberAsync();

    // Iteration operations
    Task<ProjectIteration> SubmitIterationAsync(Guid projectId, Guid studentId, string? notes);
    Task<IEnumerable<ProjectIteration>> GetIterationsAsync(Guid projectId);
    Task<ProjectIteration?> GetIterationByIdAsync(Guid iterationId);
    Task<bool> ReviewIterationAsync(Guid iterationId, Guid mentorId, bool approved, string? reviewNotes);

    // Client deliverable operations
    Task<ClientDeliverable> DeliverToClientAsync(Guid projectId, Guid mentorId, Guid iterationId, string? mentorNotes);
    Task<IEnumerable<ClientDeliverable>> GetDeliverablesAsync(Guid projectId);
    Task<bool> AcceptDeliverableAsync(Guid deliverableId, Guid clientId, string? clientNotes);
    Task<bool> RequestDeliverableRevisionAsync(Guid deliverableId, Guid clientId, string? clientNotes);

    // Messaging
    Task<ProjectMessage> SendMessageAsync(Guid projectId, Guid senderId, ProjectMessageSenderRole role, string content);
    Task<IEnumerable<ProjectMessage>> GetMessagesAsync(Guid projectId);

    // Student project queries
    Task<IEnumerable<MentorshipProject>> GetStudentProjectsAsync(Guid studentId);
}
