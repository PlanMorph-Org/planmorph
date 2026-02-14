using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Repositories;

public interface IMentorshipProjectRepository : IRepository<MentorshipProject>
{
    Task<IEnumerable<MentorshipProject>> GetProjectsByClientIdAsync(Guid clientId);
    Task<IEnumerable<MentorshipProject>> GetProjectsByMentorIdAsync(Guid mentorId);
    Task<IEnumerable<MentorshipProject>> GetProjectsByStudentIdAsync(Guid studentId);
    Task<IEnumerable<MentorshipProject>> GetProjectsByStatusAsync(ProjectStatus status);
    Task<IEnumerable<MentorshipProject>> GetPublishedProjectsAsync();
    Task<MentorshipProject?> GetProjectWithDetailsAsync(Guid projectId);
    Task<IEnumerable<MentorshipProject>> SearchProjectsAsync(string searchTerm);
    Task<int> GetActiveProjectCountByMentorAsync(Guid mentorId);
    Task<int> GetActiveProjectCountByStudentAsync(Guid studentId);
}
