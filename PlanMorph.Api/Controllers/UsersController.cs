using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using PlanMorph.Application.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;
using PlanMorph.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<UsersController> _logger;
    private readonly IConfiguration _configuration;

    public UsersController(
        UserManager<User> userManager,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        ApplicationDbContext dbContext,
        IMemoryCache memoryCache,
        ILogger<UsersController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _logger = logger;
        _configuration = configuration;
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

        if (!user.IsFoundingMember)
        {
            var archSlots = int.TryParse(_configuration["FoundingMembers:ArchitectSlots"], out var a) ? a : 25;
            var engSlots = int.TryParse(_configuration["FoundingMembers:EngineerSlots"], out var e) ? e : 25;

            if (user.Role == UserRole.Architect)
            {
                var currentCount = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Architect && u.IsFoundingMember);
                if (currentCount < archSlots)
                {
                    user.IsFoundingMember = true;
                    user.FoundingMemberSlot = currentCount + 1;
                }
            }
            else if (user.Role == UserRole.Engineer)
            {
                var currentCount = await _dbContext.Users.CountAsync(u => u.Role == UserRole.Engineer && u.IsFoundingMember);
                if (currentCount < engSlots)
                {
                    user.IsFoundingMember = true;
                    user.FoundingMemberSlot = currentCount + 1;
                }
            }
        }

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

            var existingWallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (existingWallet == null)
            {
                _dbContext.Wallets.Add(new Wallet
                {
                    UserId = user.Id,
                    Currency = "KES",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

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

    [Authorize(Roles = "Admin")]
    [HttpPost("create-student-test-user")]
    public async Task<IActionResult> CreateStudentTestUser([FromBody] CreateStudentTestUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (existingUser != null)
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        var firstName = string.IsNullOrWhiteSpace(dto.FirstName) ? "Smoke" : dto.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(dto.LastName) ? "Student" : dto.LastName.Trim();

        var user = new User
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Role = UserRole.Student,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new { message = "Failed to create student test user", errors = createResult.Errors });
        }

        await _userManager.AddToRoleAsync(user, UserRole.Student.ToString());

        var existingWallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == user.Id);
        if (existingWallet == null)
        {
            _dbContext.Wallets.Add(new Wallet
            {
                UserId = user.Id,
                Currency = "KES",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Student test user created",
            userId = user.Id,
            email = user.Email,
            role = user.Role.ToString()
        });
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
        var portal = NormalizePortal(dto.Portal);
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (IsRateLimited($"pwdreset:ip:{portal}:{clientIp}", GetIpLimit(portal), TimeSpan.FromMinutes(15)))
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        var normalizedEmail = dto.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        if (IsRateLimited($"pwdreset:email:{portal}:{normalizedEmail.ToLowerInvariant()}", GetEmailLimit(portal), TimeSpan.FromMinutes(15)))
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        if (!MatchesPortalRole(user, portal))
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        if ((user.Role == UserRole.Architect || user.Role == UserRole.Engineer) && !user.IsActive)
        {
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var isProfessional = user.Role == UserRole.Architect || user.Role == UserRole.Engineer;
        var portalPath = portal switch
        {
            "architect" => "/architect/reset-password",
            "engineer" => "/engineer/reset-password",
            _ => "/reset-password"
        };
        var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            frontendBaseUrl = "https://atelier.planmorph.software";
        }

        var resetUrl =
            $"{frontendBaseUrl}{portalPath}?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email ?? string.Empty)}";

        string? professionalVerificationCode = null;
        if (isProfessional)
        {
            professionalVerificationCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var codeRecord = new ProfessionalResetCodeRecord
            {
                CodeHash = ComputeSha256(professionalVerificationCode),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
                AttemptsRemaining = 3
            };

            _memoryCache.Set(GetProfessionalCodeCacheKey(user.Email ?? string.Empty), codeRecord, new DateTimeOffset(codeRecord.ExpiresAtUtc));
        }

        // Send password reset email
        await _emailService.SendPasswordResetEmailAsync(
            user.Email ?? "",
            user.FirstName,
            resetUrl,
            isProfessional,
            professionalVerificationCode
        );

        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var portal = NormalizePortal(dto.Portal);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid reset request" });

        if (!MatchesPortalRole(user, portal))
            return BadRequest(new { message = "Invalid reset request" });

        var isProfessional = user.Role == UserRole.Architect || user.Role == UserRole.Engineer;

        if (isProfessional)
        {
            if (string.IsNullOrWhiteSpace(dto.VerificationCode))
                return BadRequest(new { message = "Verification code is required for professional account resets" });

            if (!TryValidateProfessionalResetCode(user.Email ?? string.Empty, dto.VerificationCode.Trim()))
                return BadRequest(new { message = "Invalid or expired verification code" });

            if (!IsProfessionalPasswordStrong(dto.NewPassword))
            {
                return BadRequest(new
                {
                    message = "Professional passwords must be at least 12 characters and include uppercase, lowercase, number, and special character"
                });
            }
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to reset password", errors = result.Errors });

        await _userManager.UpdateSecurityStampAsync(user);

        return Ok(new { message = "Password reset successfully" });
    }

    private bool IsRateLimited(string key, int maxRequests, TimeSpan window)
    {
        if (maxRequests <= 0)
        {
            return false;
        }

        var currentCount = _memoryCache.Get<int?>(key) ?? 0;
        currentCount++;

        _memoryCache.Set(key, currentCount, window);
        if (currentCount > maxRequests)
        {
            _logger.LogWarning("Password reset rate limit hit for key {RateKey}", key);
            return true;
        }

        return false;
    }

    private static string NormalizePortal(string? portal)
    {
        if (string.IsNullOrWhiteSpace(portal))
        {
            return "client";
        }

        var value = portal.Trim().ToLowerInvariant();
        return value is "architect" or "engineer" ? value : "client";
    }

    private static int GetIpLimit(string portal) => portal is "architect" or "engineer" ? 8 : 20;

    private static int GetEmailLimit(string portal) => portal is "architect" or "engineer" ? 2 : 5;

    private static bool MatchesPortalRole(User user, string portal)
    {
        return portal switch
        {
            "architect" => user.Role == UserRole.Architect,
            "engineer" => user.Role == UserRole.Engineer,
            _ => user.Role == UserRole.Client || user.Role == UserRole.Student || user.Role == UserRole.Admin
        };
    }

    private bool TryValidateProfessionalResetCode(string email, string providedCode)
    {
        var cacheKey = GetProfessionalCodeCacheKey(email);
        var record = _memoryCache.Get<ProfessionalResetCodeRecord>(cacheKey);
        if (record == null || record.ExpiresAtUtc < DateTime.UtcNow)
        {
            _memoryCache.Remove(cacheKey);
            return false;
        }

        var providedHash = ComputeSha256(providedCode);
        var isMatch = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(record.CodeHash),
            Encoding.UTF8.GetBytes(providedHash)
        );

        if (!isMatch)
        {
            record.AttemptsRemaining--;
            if (record.AttemptsRemaining <= 0)
            {
                _memoryCache.Remove(cacheKey);
            }
            else
            {
                _memoryCache.Set(cacheKey, record, new DateTimeOffset(record.ExpiresAtUtc));
            }

            return false;
        }

        _memoryCache.Remove(cacheKey);
        return true;
    }

    private static string GetProfessionalCodeCacheKey(string email)
    {
        return $"prof-pwdreset-code:{email.Trim().ToLowerInvariant()}";
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static bool IsProfessionalPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            return false;
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private sealed class ProfessionalResetCodeRecord
    {
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int AttemptsRemaining { get; set; }
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
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
    public string? Portal { get; set; }
}

public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Token { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = "";

    public string? Portal { get; set; }
    public string? VerificationCode { get; set; }
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

public class CreateStudentTestUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = "";

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
