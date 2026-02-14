using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Repositories;

public interface IStudentApplicationRepository : IRepository<StudentApplication>
{
    Task<IEnumerable<StudentApplication>> GetPendingApplicationsAsync();
    Task<IEnumerable<StudentApplication>> GetApplicationsByStatusAsync(ApplicationStatus status);
    Task<StudentApplication?> GetApplicationByUserIdAsync(Guid userId);
    Task<IEnumerable<StudentApplication>> GetApplicationsByMentorInviteAsync(Guid mentorId);
    Task<bool> HasPendingApplicationAsync(Guid userId);
}
