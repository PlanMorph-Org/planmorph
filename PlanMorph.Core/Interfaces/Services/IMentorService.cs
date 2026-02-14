using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Services;

public interface IMentorService
{
    // Profile management
    Task<MentorProfile?> GetMentorProfileAsync(Guid userId);
    Task<MentorProfile> ActivateMentorProfileAsync(Guid userId, string? bio, string? specializations, int maxStudents, int maxProjects);
    Task<bool> UpdateMentorProfileAsync(Guid userId, string? bio, string? specializations, int? maxStudents, int? maxProjects, bool? acceptingStudents);
    Task<bool> IsMentorEligibleAsync(Guid userId);

    // Student management
    Task<IEnumerable<MentorStudentRelationship>> GetMentorStudentsAsync(Guid mentorId);
    Task<bool> InviteStudentAsync(Guid mentorId, string studentEmail, string studentFirstName, string studentLastName);
    Task<bool> RemoveStudentAsync(Guid mentorId, Guid studentUserId, string? reason);
    Task<int> GetActiveStudentCountAsync(Guid mentorId);

    // Stats
    Task<int> GetActiveProjectCountAsync(Guid mentorId);
}
