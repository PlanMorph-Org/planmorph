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
        string firstName, string lastName, string email,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, string? schoolIdUrl)
    {
        return await CreateApplicationAsync(
            firstName, lastName, email, phoneNumber,
            studentType, universityName, studentIdNumber, portfolioUrl,
            schoolIdUrl, ApplicationType.SelfApply, null);
    }

    public async Task<StudentApplication> SubmitInvitedApplicationAsync(
        string firstName, string lastName, string email,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, string? schoolIdUrl, Guid mentorId)
    {
        return await CreateApplicationAsync(
            firstName, lastName, email, phoneNumber,
            studentType, universityName, studentIdNumber, portfolioUrl,
            schoolIdUrl, ApplicationType.MentorInvite, mentorId);
    }

    private async Task<StudentApplication> CreateApplicationAsync(
        string firstName, string lastName, string email,
        string phoneNumber, StudentType studentType, string universityName,
        string? studentIdNumber, string? portfolioUrl, string? schoolIdUrl,
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

        var createResult = await _userManager.CreateAsync(user);
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
            SchoolIdUrl = schoolIdUrl,
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

        // Activate the user account and generate credentials
        var user = await _userManager.FindByIdAsync(application.UserId.ToString());
        if (user != null)
        {
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            // Generate a secure temporary password
            var generatedPassword = GenerateSecurePassword();
            var addPasswordResult = await _userManager.AddPasswordAsync(user, generatedPassword);
            if (!addPasswordResult.Succeeded)
            {
                _logger.LogError("Failed to set generated password for student {UserId}: {Errors}",
                    user.Id, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
            }

            // Send credentials email instead of generic approval email
            await _emailService.SendStudentCredentialsEmailAsync(
                user.Email!, user.FirstName, generatedPassword);
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

    private static string GenerateSecurePassword(int length = 16)
    {
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%&*";
        const string allChars = upperChars + lowerChars + digitChars + specialChars;

        var random = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[length];
        random.GetBytes(bytes);

        var chars = new char[length];
        // Guarantee at least one of each required type
        chars[0] = upperChars[bytes[0] % upperChars.Length];
        chars[1] = lowerChars[bytes[1] % lowerChars.Length];
        chars[2] = digitChars[bytes[2] % digitChars.Length];
        chars[3] = specialChars[bytes[3] % specialChars.Length];

        for (int i = 4; i < length; i++)
            chars[i] = allChars[bytes[i] % allChars.Length];

        // Shuffle the result
        for (int i = chars.Length - 1; i > 0; i--)
        {
            var swapBytes = new byte[1];
            random.GetBytes(swapBytes);
            int j = swapBytes[0] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
