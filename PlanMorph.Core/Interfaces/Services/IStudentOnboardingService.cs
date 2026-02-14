using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Services;

public interface IStudentOnboardingService
{
    // Student application
    Task<StudentApplication> SubmitApplicationAsync(
        string firstName, string lastName, string email,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, string? schoolIdUrl);

    Task<StudentApplication> SubmitInvitedApplicationAsync(
        string firstName, string lastName, string email,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, string? schoolIdUrl, Guid mentorId);

    // Admin review
    Task<StudentApplication?> GetApplicationByIdAsync(Guid applicationId);
    Task<IEnumerable<StudentApplication>> GetPendingApplicationsAsync();
    Task<IEnumerable<StudentApplication>> GetApplicationsByStatusAsync(ApplicationStatus status);
    Task<bool> ApproveApplicationAsync(Guid applicationId, Guid adminId, string? reviewNotes);
    Task<bool> RejectApplicationAsync(Guid applicationId, Guid adminId, string? reviewNotes);

    // Student profile
    Task<StudentProfile?> GetStudentProfileAsync(Guid userId);
    Task<StudentProfile?> GetStudentProfileByIdAsync(Guid profileId);
    Task<bool> UpdateStudentProfileAsync(Guid userId, string? universityName, EnrollmentStatus? enrollmentStatus, DateTime? expectedGraduation);
    Task<bool> HasPendingApplicationAsync(Guid userId);
}
