using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _dbContext;

    public UsersController(UserManager<User> userManager, IEmailService emailService, IFileStorageService fileStorageService, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var users = _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToList();

        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending-architects")]
    public IActionResult GetPendingArchitects()
    {
        var pendingProfessionals = _userManager.Users
            .Where(u => (u.Role == UserRole.Architect || u.Role == UserRole.Engineer) && !u.IsActive && !u.IsRejected)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToList();

        return Ok(pendingProfessionals);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/approve-architect")]
    public async Task<IActionResult> ApproveArchitect(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (user.Role != UserRole.Architect && user.Role != UserRole.Engineer)
            return BadRequest(new { message = "User is not an architect or engineer" });

        if (user.IsActive)
            return BadRequest(new { message = "Professional is already active" });

        var admin = await _userManager.GetUserAsync(User);

        user.IsActive = true;
        user.IsRejected = false;
        user.RejectionReason = null;
        user.RejectedAt = null;
        user.RejectedById = null;
        if (admin != null)
        {
            user.LastReviewedAt = DateTime.UtcNow;
            user.LastReviewedById = admin.Id;
            user.LastReviewedByName = $"{admin.FirstName} {admin.LastName}";
        }
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to approve architect", errors = result.Errors });

        // Send approval email
        if (user.Email != null)
        {
            await _emailService.SendArchitectApprovedEmailAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}"
            );
        }

        if (admin != null)
        {
            _dbContext.ProfessionalReviewLogs.Add(new ProfessionalReviewLog
            {
                ProfessionalUserId = user.Id,
                AdminUserId = admin.Id,
                Action = ProfessionalReviewAction.Approved,
                Notes = user.VerificationNotes,
                LicenseVerified = user.LicenseVerified,
                DocumentsVerified = user.DocumentsVerified,
                ExperienceVerified = user.ExperienceVerified,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new { message = "Architect approved successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/reject-architect")]
    public async Task<IActionResult> RejectArchitect(Guid id, [FromBody] RejectionReasonDto? rejectionDto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (user.Role != UserRole.Architect && user.Role != UserRole.Engineer)
            return BadRequest(new { message = "User is not an architect or engineer" });

        // Send rejection email before deleting
        if (user.Email != null)
        {
            await _emailService.SendArchitectRejectedEmailAsync(
                user.Email,
                $"{user.FirstName} {user.LastName}",
                rejectionDto?.Reason
            );
        }

        var admin = await _userManager.GetUserAsync(User);
        user.IsActive = false;
        user.IsRejected = true;
        user.RejectionReason = rejectionDto?.Reason?.Trim();
        user.RejectedAt = DateTime.UtcNow;
        user.RejectedById = admin?.Id;
        if (admin != null)
        {
            user.LastReviewedAt = DateTime.UtcNow;
            user.LastReviewedById = admin.Id;
            user.LastReviewedByName = $"{admin.FirstName} {admin.LastName}";
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to reject architect", errors = result.Errors });

        if (admin != null)
        {
            _dbContext.ProfessionalReviewLogs.Add(new ProfessionalReviewLog
            {
                ProfessionalUserId = user.Id,
                AdminUserId = admin.Id,
                Action = ProfessionalReviewAction.Rejected,
                Reason = user.RejectionReason,
                Notes = user.VerificationNotes,
                LicenseVerified = user.LicenseVerified,
                DocumentsVerified = user.DocumentsVerified,
                ExperienceVerified = user.ExperienceVerified,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new { message = "Architect application rejected" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to activate user", errors = result.Errors });

        return Ok(new { message = "User activated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to deactivate user", errors = result.Errors });

        return Ok(new { message = "User deactivated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}/professional-documents")]
    public async Task<IActionResult> GetProfessionalDocuments(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            return NotFound(new { message = "User not found" });

        if (user.Role != UserRole.Architect && user.Role != UserRole.Engineer)
            return BadRequest(new { message = "User is not an architect or engineer" });

        var response = new ProfessionalDocumentsDto
        {
            CvUrl = await GetSignedUrlAsync(user.CvUrl),
            CvFileName = user.CvFileName,
            CvFileSizeBytes = user.CvFileSizeBytes,
            CvUploadedAt = user.CvUploadedAt,
            CoverLetterUrl = await GetSignedUrlAsync(user.CoverLetterUrl),
            CoverLetterFileName = user.CoverLetterFileName,
            CoverLetterFileSizeBytes = user.CoverLetterFileSizeBytes,
            CoverLetterUploadedAt = user.CoverLetterUploadedAt,
            WorkExperienceUrl = await GetSignedUrlAsync(user.WorkExperienceUrl),
            WorkExperienceFileName = user.WorkExperienceFileName,
            WorkExperienceFileSizeBytes = user.WorkExperienceFileSizeBytes,
            WorkExperienceUploadedAt = user.WorkExperienceUploadedAt
        };

        return Ok(response);
    }

    private async Task<string?> GetSignedUrlAsync(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        return await _fileStorageService.GetSignedUrlAsync(fileUrl, 60);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Send password reset email
        await _emailService.SendPasswordResetEmailAsync(
            user.Email ?? "",
            user.FirstName,
            resetToken
        );

        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid reset request" });

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to reset password", errors = result.Errors });

        return Ok(new { message = "Password reset successfully" });
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RejectionReasonDto
{
    public string? Reason { get; set; }
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = "";
}

public class ResetPasswordDto
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";
    public string NewPassword { get; set; } = "";
}

public class ProfessionalDocumentsDto
{
    public string? CvUrl { get; set; }
    public string? CvFileName { get; set; }
    public long? CvFileSizeBytes { get; set; }
    public DateTime? CvUploadedAt { get; set; }
    public string? CoverLetterUrl { get; set; }
    public string? CoverLetterFileName { get; set; }
    public long? CoverLetterFileSizeBytes { get; set; }
    public DateTime? CoverLetterUploadedAt { get; set; }
    public string? WorkExperienceUrl { get; set; }
    public string? WorkExperienceFileName { get; set; }
    public long? WorkExperienceFileSizeBytes { get; set; }
    public DateTime? WorkExperienceUploadedAt { get; set; }
}
