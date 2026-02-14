using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Application.Services;

public class MentorService : IMentorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<MentorService> _logger;

    public MentorService(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<MentorService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<MentorProfile?> GetMentorProfileAsync(Guid userId)
    {
        return await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<bool> IsMentorEligibleAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            return false;

        // Must be an Architect or Engineer
        if (user.Role != UserRole.Architect && user.Role != UserRole.Engineer)
            return false;

        // Must have at least 1 approved design OR 5 completed verifications
        var approvedDesigns = await _unitOfWork.Designs
            .CountAsync(d => d.ArchitectId == userId && d.Status == DesignStatus.Approved);

        if (approvedDesigns >= 1)
            return true;

        var completedVerifications = await _unitOfWork.DesignVerifications
            .CountAsync(v => v.VerifierUserId == userId && v.Status == VerificationStatus.Verified);

        return completedVerifications >= 5;
    }

    public async Task<MentorProfile> ActivateMentorProfileAsync(
        Guid userId, string? bio, string? specializations, int maxStudents, int maxProjects)
    {
        // Check if profile already exists
        var existing = await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (existing != null)
            throw new InvalidOperationException("Mentor profile already exists.");

        if (!await IsMentorEligibleAsync(userId))
            throw new InvalidOperationException("User does not meet mentor eligibility requirements.");

        var profile = new MentorProfile
        {
            UserId = userId,
            Bio = bio ?? string.Empty,
            Specializations = specializations ?? string.Empty,
            MaxConcurrentStudents = maxStudents,
            MaxConcurrentProjects = maxProjects,
            AcceptingStudents = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.MentorProfiles.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Mentor profile activated for user {UserId}", userId);

        return profile;
    }

    public async Task<bool> UpdateMentorProfileAsync(
        Guid userId, string? bio, string? specializations,
        int? maxStudents, int? maxProjects, bool? acceptingStudents)
    {
        var profile = await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return false;

        if (bio != null)
            profile.Bio = bio;
        if (specializations != null)
            profile.Specializations = specializations;
        if (maxStudents.HasValue)
            profile.MaxConcurrentStudents = maxStudents.Value;
        if (maxProjects.HasValue)
            profile.MaxConcurrentProjects = maxProjects.Value;
        if (acceptingStudents.HasValue)
            profile.AcceptingStudents = acceptingStudents.Value;

        profile.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.MentorProfiles.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<MentorStudentRelationship>> GetMentorStudentsAsync(Guid mentorId)
    {
        return await _unitOfWork.MentorStudentRelationships
            .FindAsync(r => r.MentorId == mentorId && r.Status != RelationshipStatus.Ended);
    }

    public async Task<int> GetActiveStudentCountAsync(Guid mentorId)
    {
        return await _unitOfWork.MentorStudentRelationships
            .CountAsync(r => r.MentorId == mentorId && r.Status == RelationshipStatus.Active);
    }

    public async Task<bool> InviteStudentAsync(
        Guid mentorId, string studentEmail, string studentFirstName, string studentLastName)
    {
        var mentor = await _userManager.FindByIdAsync(mentorId.ToString());
        if (mentor == null)
            return false;

        // Verify mentor has an active profile
        var mentorProfile = await _unitOfWork.MentorProfiles
            .FirstOrDefaultAsync(p => p.UserId == mentorId);

        if (mentorProfile == null)
            throw new InvalidOperationException("You must activate your mentor profile first.");

        if (!mentorProfile.AcceptingStudents)
            throw new InvalidOperationException("Your profile is not currently accepting students.");

        // Check capacity
        var activeCount = await GetActiveStudentCountAsync(mentorId);
        if (activeCount >= mentorProfile.MaxConcurrentStudents)
            throw new InvalidOperationException(
                $"You have reached your maximum of {mentorProfile.MaxConcurrentStudents} concurrent students.");

        // Check if student email already has an account
        var existingUser = await _userManager.FindByEmailAsync(studentEmail);
        if (existingUser != null)
            throw new InvalidOperationException("A user with this email already exists on the platform.");

        // Send invitation email
        var mentorName = $"{mentor.FirstName} {mentor.LastName}";
        await _emailService.SendMentorStudentInvitationEmailAsync(
            studentEmail, studentFirstName, mentorName);

        _logger.LogInformation(
            "Mentor {MentorId} sent invitation to {Email}", mentorId, studentEmail);

        return true;
    }

    public async Task<bool> RemoveStudentAsync(Guid mentorId, Guid studentUserId, string? reason)
    {
        var relationship = await _unitOfWork.MentorStudentRelationships
            .FirstOrDefaultAsync(r => r.MentorId == mentorId && r.StudentId == studentUserId
                && r.Status != RelationshipStatus.Ended);

        if (relationship == null)
            return false;

        relationship.Status = RelationshipStatus.Ended;
        relationship.EndedAt = DateTime.UtcNow;
        relationship.EndReason = reason;
        relationship.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.MentorStudentRelationships.UpdateAsync(relationship);

        // Update student profile mentorship status
        var studentProfile = await _unitOfWork.StudentProfiles
            .FirstOrDefaultAsync(p => p.UserId == studentUserId);

        if (studentProfile != null && studentProfile.MentorId == mentorId)
        {
            studentProfile.MentorId = null;
            studentProfile.MentorshipStatus = MentorshipStatus.Unmatched;
            studentProfile.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.StudentProfiles.UpdateAsync(studentProfile);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Mentor {MentorId} removed student {StudentId}. Reason: {Reason}",
            mentorId, studentUserId, reason ?? "Not specified");

        return true;
    }

    public async Task<int> GetActiveProjectCountAsync(Guid mentorId)
    {
        return await _unitOfWork.MentorshipProjectRepository
            .GetActiveProjectCountByMentorAsync(mentorId);
    }
}
