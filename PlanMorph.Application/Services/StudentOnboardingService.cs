using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;
using PlanMorph.Application.Services;

namespace PlanMorph.Application.Services;

public class StudentOnboardingService : IStudentOnboardingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<StudentOnboardingService> _logger;

    public StudentOnboardingService(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<StudentOnboardingService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<StudentApplication> SubmitApplicationAsync(
        string firstName, string lastName, string email, string password,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl)
    {
        return await CreateApplicationAsync(
            firstName, lastName, email, password, phoneNumber,
            studentType, universityName, studentIdNumber, portfolioUrl,
            ApplicationType.SelfApply, null);
    }

    public async Task<StudentApplication> SubmitInvitedApplicationAsync(
        string firstName, string lastName, string email, string password,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, Guid mentorId)
    {
        return await CreateApplicationAsync(
            firstName, lastName, email, password, phoneNumber,
            studentType, universityName, studentIdNumber, portfolioUrl,
            ApplicationType.MentorInvite, mentorId);
    }

    private async Task<StudentApplication> CreateApplicationAsync(
        string firstName, string lastName, string email, string password,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl,
        ApplicationType applicationType, Guid? mentorId)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        // Create user account (inactive until approved)
        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Role = UserRole.Student,
            IsActive = false,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create account: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Student");

        // Create application record
        var application = new StudentApplication
        {
            UserId = user.Id,
            ApplicationType = applicationType,
            InvitedByMentorId = mentorId,
            StudentType = studentType,
            UniversityName = universityName,
            StudentIdNumber = studentIdNumber,
            PortfolioUrl = portfolioUrl,
            Status = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.StudentApplications.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();

        // Send notifications
        await _emailService.SendStudentApplicationReceivedEmailAsync(email, firstName);
        await _emailService.SendAdminNewStudentApplicationEmailAsync(
            $"{firstName} {lastName}", studentType.ToString(), universityName);

        _logger.LogInformation("Student application submitted for {Email}, type: {Type}", email, applicationType);

        return application;
    }

    public async Task<StudentApplication?> GetApplicationByIdAsync(Guid applicationId)
    {
        return await _unitOfWork.StudentApplications.GetByIdAsync(applicationId);
    }

    public async Task<IEnumerable<StudentApplication>> GetPendingApplicationsAsync()
    {
        return await _unitOfWork.StudentApplicationRepository.GetPendingApplicationsAsync();
    }

    public async Task<IEnumerable<StudentApplication>> GetApplicationsByStatusAsync(ApplicationStatus status)
    {
        return await _unitOfWork.StudentApplicationRepository.GetApplicationsByStatusAsync(status);
    }

    public async Task<bool> ApproveApplicationAsync(Guid applicationId, Guid adminId, string? reviewNotes)
    {
        var application = await _unitOfWork.StudentApplicationRepository
            .GetApplicationByUserIdAsync(applicationId);

        // Try by application ID if not found by user ID
        application ??= await _unitOfWork.StudentApplications.GetByIdAsync(applicationId);

        if (application == null || application.Status != ApplicationStatus.Pending)
            return false;

        application.Status = ApplicationStatus.Approved;
        application.ReviewedById = adminId;
        application.ReviewNotes = reviewNotes;
        application.ReviewedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StudentApplications.UpdateAsync(application);

        // Activate the user account
        var user = await _userManager.FindByIdAsync(application.UserId.ToString());
        if (user != null)
        {
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
        }

        // Create student profile
        var profile = new StudentProfile
        {
            UserId = application.UserId,
            StudentType = application.StudentType,
            UniversityName = application.UniversityName,
            StudentIdNumber = application.StudentIdNumber,
            EnrollmentStatus = EnrollmentStatus.Enrolled,
            MentorId = application.InvitedByMentorId,
            MentorshipStatus = application.InvitedByMentorId.HasValue
                ? MentorshipStatus.Matched
                : MentorshipStatus.Unmatched,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.StudentProfiles.AddAsync(profile);

        // If invited by mentor, create the relationship
        if (application.InvitedByMentorId.HasValue)
        {
            var relationship = new MentorStudentRelationship
            {
                MentorId = application.InvitedByMentorId.Value,
                StudentId = application.UserId,
                Status = RelationshipStatus.Active,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MentorStudentRelationships.AddAsync(relationship);
        }

        await _unitOfWork.SaveChangesAsync();

        // Send approval email
        if (user != null)
        {
            await _emailService.SendStudentApplicationApprovedEmailAsync(
                user.Email!, user.FirstName);
        }

        _logger.LogInformation("Student application {Id} approved by admin {AdminId}", applicationId, adminId);

        return true;
    }

    public async Task<bool> RejectApplicationAsync(Guid applicationId, Guid adminId, string? reviewNotes)
    {
        var application = await _unitOfWork.StudentApplications.GetByIdAsync(applicationId);

        if (application == null || application.Status != ApplicationStatus.Pending)
            return false;

        application.Status = ApplicationStatus.Rejected;
        application.ReviewedById = adminId;
        application.ReviewNotes = reviewNotes;
        application.ReviewedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StudentApplications.UpdateAsync(application);
        await _unitOfWork.SaveChangesAsync();

        // Send rejection email
        var user = await _userManager.FindByIdAsync(application.UserId.ToString());
        if (user != null)
        {
            await _emailService.SendStudentApplicationRejectedEmailAsync(
                user.Email!, user.FirstName, reviewNotes);
        }

        _logger.LogInformation("Student application {Id} rejected by admin {AdminId}", applicationId, adminId);

        return true;
    }

    public async Task<StudentProfile?> GetStudentProfileAsync(Guid userId)
    {
        return await _unitOfWork.StudentProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<StudentProfile?> GetStudentProfileByIdAsync(Guid profileId)
    {
        return await _unitOfWork.StudentProfiles.GetByIdAsync(profileId);
    }

    public async Task<bool> UpdateStudentProfileAsync(Guid userId, string? universityName,
        EnrollmentStatus? enrollmentStatus, DateTime? expectedGraduation)
    {
        var profile = await _unitOfWork.StudentProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return false;

        if (universityName != null)
            profile.UniversityName = universityName;
        if (enrollmentStatus.HasValue)
            profile.EnrollmentStatus = enrollmentStatus.Value;
        if (expectedGraduation.HasValue)
            profile.ExpectedGraduation = expectedGraduation.Value;

        profile.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.StudentProfiles.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasPendingApplicationAsync(Guid userId)
    {
        return await _unitOfWork.StudentApplicationRepository.HasPendingApplicationAsync(userId);
    }
}
