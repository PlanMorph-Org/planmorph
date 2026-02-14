using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Services;

public interface IProjectWorkflowService
{
    /// <summary>
    /// Validates and performs a state transition on a mentorship project.
    /// </summary>
    Task<bool> TransitionAsync(Guid projectId, ProjectStatus newStatus, Guid actorId, string? notes = null);

    /// <summary>
    /// Gets the valid next states from the current state.
    /// </summary>
    IEnumerable<ProjectStatus> GetAllowedTransitions(ProjectStatus currentStatus);

    /// <summary>
    /// Checks if a transition from current to target state is valid.
    /// </summary>
    bool IsValidTransition(ProjectStatus from, ProjectStatus to);
}
