using Microsoft.AspNetCore.Identity;
using PlanMorph.Application.DTOs.Auth;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return null; // User already exists
        }

        // Determine user role and activation status
        var isArchitect = string.Equals(registerDto.Role, "Architect", StringComparison.OrdinalIgnoreCase);
        var isEngineer = string.Equals(registerDto.Role, "Engineer", StringComparison.OrdinalIgnoreCase);
        var isProfessional = isArchitect || isEngineer;
        var userRole = isArchitect ? UserRole.Architect
                     : isEngineer ? UserRole.Engineer
                     : UserRole.Client;

        if (isProfessional)
        {
            if (string.IsNullOrWhiteSpace(registerDto.ProfessionalLicense) ||
                !registerDto.YearsOfExperience.HasValue ||
                registerDto.YearsOfExperience.Value <= 0)
            {
                return null;
            }

            if (!HasPortfolioOrDocuments(registerDto))
            {
                return null;
            }
        }

        // Create new user
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            Role = userRole,
            EmailConfirmed = true,
            IsActive = !isProfessional, // Architects and Engineers need admin approval
            ProfessionalLicense = registerDto.ProfessionalLicense?.Trim(),
            YearsOfExperience = registerDto.YearsOfExperience,
            PortfolioUrl = registerDto.PortfolioUrl?.Trim(),
            Specialization = registerDto.Specialization?.Trim(),
            CvUrl = registerDto.CvUrl?.Trim(),
            CoverLetterUrl = registerDto.CoverLetterUrl?.Trim(),
            WorkExperienceUrl = registerDto.WorkExperienceUrl?.Trim(),
            CvFileName = registerDto.CvFileName?.Trim(),
            CvFileSizeBytes = registerDto.CvFileSizeBytes,
            CvUploadedAt = registerDto.CvUploadedAt,
            CoverLetterFileName = registerDto.CoverLetterFileName?.Trim(),
            CoverLetterFileSizeBytes = registerDto.CoverLetterFileSizeBytes,
            CoverLetterUploadedAt = registerDto.CoverLetterUploadedAt,
            WorkExperienceFileName = registerDto.WorkExperienceFileName?.Trim(),
            WorkExperienceFileSizeBytes = registerDto.WorkExperienceFileSizeBytes,
            WorkExperienceUploadedAt = registerDto.WorkExperienceUploadedAt
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return null;
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, userRole.ToString());

        // If professional (architect/engineer), return response indicating pending approval
        if (isProfessional)
        {
            // Send notification that application is being reviewed (optional)
            // For now, we don't send an email for architects pending approval

            return new AuthResponseDto
            {
                Token = "", // No token for inactive accounts
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = "PendingApproval"
            };
        }

        // For clients, send welcome email and generate token immediately
        if (user.Email != null)
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
        }

        var token = _tokenService.GenerateToken(user, userRole.ToString());

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = userRole.ToString()
        };
    }

    private static bool HasPortfolioOrDocuments(RegisterDto registerDto)
    {
        if (!string.IsNullOrWhiteSpace(registerDto.PortfolioUrl))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(registerDto.CvUrl)
               && !string.IsNullOrWhiteSpace(registerDto.CoverLetterUrl)
               && !string.IsNullOrWhiteSpace(registerDto.WorkExperienceUrl);
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            // User exists but is not active (e.g., pending architect)
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        
        if (!result.Succeeded)
        {
            return null;
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? UserRole.Client.ToString();

        // Generate token
        var token = _tokenService.GenerateToken(user, role);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role
        };
    }
}
