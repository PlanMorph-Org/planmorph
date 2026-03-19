using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IEmailService emailService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

    public async Task<AuthResponseDto?> RegisterProfessionalWithGoogleAsync(GoogleProfessionalRegisterDto registerDto)
    {
        var isArchitect = string.Equals(registerDto.Role, "Architect", StringComparison.OrdinalIgnoreCase);
        var isEngineer = string.Equals(registerDto.Role, "Engineer", StringComparison.OrdinalIgnoreCase);
        if (!isArchitect && !isEngineer)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(registerDto.ProfessionalLicense) || registerDto.YearsOfExperience <= 0)
        {
            return null;
        }

        if (!HasPortfolioOrDocuments(registerDto))
        {
            return null;
        }

        var googleProfile = await ValidateGoogleIdTokenAsync(registerDto.GoogleIdToken);
        if (googleProfile == null || string.IsNullOrWhiteSpace(googleProfile.Email))
        {
            return null;
        }

        var existingUser = await _userManager.FindByEmailAsync(googleProfile.Email);
        if (existingUser != null)
        {
            return null;
        }

        var userRole = isArchitect ? UserRole.Architect : UserRole.Engineer;
        var user = new User
        {
            UserName = googleProfile.Email,
            Email = googleProfile.Email,
            EmailConfirmed = true,
            FirstName = !string.IsNullOrWhiteSpace(registerDto.FirstName)
                ? registerDto.FirstName.Trim()
                : (googleProfile.GivenName ?? string.Empty),
            LastName = !string.IsNullOrWhiteSpace(registerDto.LastName)
                ? registerDto.LastName.Trim()
                : (googleProfile.FamilyName ?? string.Empty),
            PhoneNumber = registerDto.PhoneNumber,
            Role = userRole,
            IsActive = false,
            ProfessionalLicense = registerDto.ProfessionalLicense.Trim(),
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

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return null;
        }

        await _userManager.AddToRoleAsync(user, userRole.ToString());

        return new AuthResponseDto
        {
            Token = string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = "PendingApproval"
        };
    }

    public async Task<AuthResponseDto?> LoginProfessionalWithGoogleAsync(GoogleProfessionalLoginDto loginDto)
    {
        var googleProfile = await ValidateGoogleIdTokenAsync(loginDto.GoogleIdToken);
        if (googleProfile == null || string.IsNullOrWhiteSpace(googleProfile.Email))
        {
            return null;
        }

        var user = await _userManager.FindByEmailAsync(googleProfile.Email);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        var isProfessional = string.Equals(role, UserRole.Architect.ToString(), StringComparison.OrdinalIgnoreCase)
                             || string.Equals(role, UserRole.Engineer.ToString(), StringComparison.OrdinalIgnoreCase);
        if (!isProfessional)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(loginDto.Role) && !string.Equals(loginDto.Role, role, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = _tokenService.GenerateToken(user, role);
        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role
        };
    }

    public async Task<AuthResponseDto?> RegisterClientWithGoogleAsync(GoogleClientRegisterDto registerDto)
    {
        var googleProfile = await ValidateGoogleIdTokenAsync(registerDto.GoogleIdToken);
        if (googleProfile == null || string.IsNullOrWhiteSpace(googleProfile.Email))
        {
            return null;
        }

        var existingUser = await _userManager.FindByEmailAsync(googleProfile.Email);
        if (existingUser != null)
        {
            return null;
        }

        var user = new User
        {
            UserName = googleProfile.Email,
            Email = googleProfile.Email,
            EmailConfirmed = true,
            FirstName = !string.IsNullOrWhiteSpace(registerDto.FirstName)
                ? registerDto.FirstName.Trim()
                : (googleProfile.GivenName ?? string.Empty),
            LastName = !string.IsNullOrWhiteSpace(registerDto.LastName)
                ? registerDto.LastName.Trim()
                : (googleProfile.FamilyName ?? string.Empty),
            PhoneNumber = registerDto.PhoneNumber,
            Role = UserRole.Client,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return null;
        }

        await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());

        if (user.Email != null)
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
        }

        var token = _tokenService.GenerateToken(user, UserRole.Client.ToString());
        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = UserRole.Client.ToString()
        };
    }

    public async Task<AuthResponseDto?> LoginClientWithGoogleAsync(GoogleClientLoginDto loginDto)
    {
        var googleProfile = await ValidateGoogleIdTokenAsync(loginDto.GoogleIdToken);
        if (googleProfile == null || string.IsNullOrWhiteSpace(googleProfile.Email))
        {
            return null;
        }

        var user = await _userManager.FindByEmailAsync(googleProfile.Email);
        if (user == null)
        {
            user = new User
            {
                UserName = googleProfile.Email,
                Email = googleProfile.Email,
                EmailConfirmed = true,
                FirstName = googleProfile.GivenName ?? string.Empty,
                LastName = googleProfile.FamilyName ?? string.Empty,
                Role = UserRole.Client,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return null;
            }

            await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());

            if (user.Email != null)
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
            }
        }

        if (!user.IsActive)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (!string.Equals(role, UserRole.Client.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            if (roles.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
            }
            else
            {
                return null;
            }
        }

        var token = _tokenService.GenerateToken(user, UserRole.Client.ToString());
        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = UserRole.Client.ToString()
        };
    }

    private async Task<GoogleTokenInfo?> ValidateGoogleIdTokenAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        var expectedClientId = _configuration["GoogleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(expectedClientId))
        {
            return null;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            using var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var tokenInfo = await JsonSerializer.DeserializeAsync<GoogleTokenInfo>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenInfo == null)
            {
                return null;
            }

            if (!string.Equals(tokenInfo.Aud, expectedClientId, StringComparison.Ordinal))
            {
                return null;
            }

            var issuerIsValid = string.Equals(tokenInfo.Iss, "accounts.google.com", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(tokenInfo.Iss, "https://accounts.google.com", StringComparison.OrdinalIgnoreCase);
            if (!issuerIsValid)
            {
                return null;
            }

            if (!string.Equals(tokenInfo.EmailVerified, "true", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return tokenInfo;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool HasPortfolioOrDocuments(GoogleProfessionalRegisterDto registerDto)
    {
        if (!string.IsNullOrWhiteSpace(registerDto.PortfolioUrl))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(registerDto.CvUrl)
               && !string.IsNullOrWhiteSpace(registerDto.CoverLetterUrl)
               && !string.IsNullOrWhiteSpace(registerDto.WorkExperienceUrl);
    }

    private sealed class GoogleTokenInfo
    {
        [JsonPropertyName("iss")]
        public string? Iss { get; set; }

        [JsonPropertyName("aud")]
        public string? Aud { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public string? EmailVerified { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }
}
