using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using PlanMorph.Application.DTOs.Auth;
using PlanMorph.Application.Services;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IFileStorageService _fileStorageService;

    public AuthController(IAuthService authService, IFileStorageService fileStorageService)
    {
        _authService = authService;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var isArchitect = string.Equals(registerDto.Role, "Architect", StringComparison.OrdinalIgnoreCase);
        var isEngineer = string.Equals(registerDto.Role, "Engineer", StringComparison.OrdinalIgnoreCase);
        if (isArchitect || isEngineer)
        {
            if (string.IsNullOrWhiteSpace(registerDto.ProfessionalLicense) ||
                !registerDto.YearsOfExperience.HasValue ||
                registerDto.YearsOfExperience.Value <= 0)
            {
                return BadRequest(new { message = "Professional license and years of experience are required." });
            }

            if (!HasPortfolioOrDocuments(registerDto))
            {
                return BadRequest(new
                {
                    message = "Provide a portfolio URL or upload CV, cover letter, and work experience PDF."
                });
            }
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
        {
            return BadRequest(new { message = "User registration failed. Email may already be in use." });
        }

        return Ok(result);
    }

    [HttpPost("register-professional")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegisterProfessional([FromForm] ProfessionalRegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!IsProfessionalRole(request.Role))
        {
            return BadRequest(new { message = "Role must be Architect or Engineer for this endpoint." });
        }

        if (string.IsNullOrWhiteSpace(request.ProfessionalLicense) ||
            request.YearsOfExperience <= 0)
        {
            return BadRequest(new { message = "Professional license and years of experience are required." });
        }

        var hasPortfolio = !string.IsNullOrWhiteSpace(request.PortfolioUrl);
        if (!hasPortfolio)
        {
            if (request.CvFile == null || request.CoverLetterFile == null || request.WorkExperienceFile == null)
            {
                return BadRequest(new
                {
                    message = "Provide a portfolio URL or upload CV, cover letter, and work experience PDF."
                });
            }

            if (!IsPdf(request.CvFile) || !IsPdf(request.CoverLetterFile) || !IsPdf(request.WorkExperienceFile))
            {
                return BadRequest(new { message = "CV, cover letter, and work experience must be PDF files." });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PortfolioUrl))
        {
            if (!Uri.TryCreate(request.PortfolioUrl, UriKind.Absolute, out var portfolioUri) ||
                (portfolioUri.Scheme != Uri.UriSchemeHttps && portfolioUri.Scheme != Uri.UriSchemeHttp))
            {
                return BadRequest(new { message = "Portfolio URL must be a valid http(s) URL." });
            }
        }

        var uploadBatchId = Guid.NewGuid().ToString("N");
        string? cvUrl = null;
        string? coverLetterUrl = null;
        string? workExperienceUrl = null;

        if (request.CvFile != null)
        {
            cvUrl = await UploadProfessionalDocumentAsync(request.CvFile, uploadBatchId, "cv");
        }

        if (request.CoverLetterFile != null)
        {
            coverLetterUrl = await UploadProfessionalDocumentAsync(request.CoverLetterFile, uploadBatchId, "cover-letter");
        }

        if (request.WorkExperienceFile != null)
        {
            workExperienceUrl = await UploadProfessionalDocumentAsync(request.WorkExperienceFile, uploadBatchId, "work-experience");
        }

        var registerDto = new RegisterDto
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            ProfessionalLicense = request.ProfessionalLicense,
            YearsOfExperience = request.YearsOfExperience,
            PortfolioUrl = request.PortfolioUrl,
            Specialization = request.Specialization,
            CvUrl = cvUrl,
            CoverLetterUrl = coverLetterUrl,
            WorkExperienceUrl = workExperienceUrl,
            CvFileName = request.CvFile?.FileName,
            CvFileSizeBytes = request.CvFile?.Length,
            CvUploadedAt = request.CvFile != null ? DateTime.UtcNow : null,
            CoverLetterFileName = request.CoverLetterFile?.FileName,
            CoverLetterFileSizeBytes = request.CoverLetterFile?.Length,
            CoverLetterUploadedAt = request.CoverLetterFile != null ? DateTime.UtcNow : null,
            WorkExperienceFileName = request.WorkExperienceFile?.FileName,
            WorkExperienceFileSizeBytes = request.WorkExperienceFile?.Length,
            WorkExperienceUploadedAt = request.WorkExperienceFile != null ? DateTime.UtcNow : null
        };

        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
        {
            await DeleteUploadedDocumentsAsync(cvUrl, coverLetterUrl, workExperienceUrl);
            return BadRequest(new { message = "User registration failed. Email may already be in use." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
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

    private static bool IsProfessionalRole(string? role)
    {
        return string.Equals(role, "Architect", StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, "Engineer", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPdf(IFormFile file)
    {
        if (file.Length == 0)
        {
            return false;
        }

        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        var fileName = file.FileName?.ToLowerInvariant() ?? string.Empty;
        return contentType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> UploadProfessionalDocumentAsync(IFormFile file, string batchId, string docType)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var folder = $"professionals/{batchId}/{docType}";

        using var stream = file.OpenReadStream();
        return await _fileStorageService.UploadFileAsync(
            stream,
            fileName,
            folder,
            file.ContentType ?? "application/pdf",
            false
        );
    }

    private async Task DeleteUploadedDocumentsAsync(params string?[] documentUrls)
    {
        foreach (var documentUrl in documentUrls)
        {
            if (string.IsNullOrWhiteSpace(documentUrl))
            {
                continue;
            }

            await _fileStorageService.DeleteFileAsync(documentUrl);
        }
    }

    public class ProfessionalRegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ProfessionalLicense { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Specialization { get; set; }
        public IFormFile? CvFile { get; set; }
        public IFormFile? CoverLetterFile { get; set; }
        public IFormFile? WorkExperienceFile { get; set; }
    }
}
