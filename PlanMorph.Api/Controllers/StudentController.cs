using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentOnboardingService _onboardingService;
    private readonly IMentorshipService _mentorshipService;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public StudentController(
        IStudentOnboardingService onboardingService,
        IMentorshipService mentorshipService,
        UserManager<User> userManager,
        IFileStorageService fileStorageService)
    {
        _onboardingService = onboardingService;
        _mentorshipService = mentorshipService;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Submit a new student application (public - no auth required).
    /// </summary>
    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromForm] CreateStudentApplicationDto dto, IFormFile? schoolIdFile)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Upload school ID if provided
            string? schoolIdUrl = null;
            if (schoolIdFile != null && schoolIdFile.Length > 0)
            {
                using var stream = schoolIdFile.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(schoolIdFile.FileName)}";
                schoolIdUrl = await _fileStorageService.UploadFileAsync(
                    stream, fileName, "student-applications/school-ids", schoolIdFile.ContentType);
            }

            var application = await _onboardingService.SubmitApplicationAsync(
                dto.FirstName, dto.LastName, dto.Email,
                dto.PhoneNumber ?? "", dto.StudentType, dto.UniversityName,
                dto.StudentIdNumber, dto.PortfolioUrl, schoolIdUrl);

            return Ok(new
            {
                message = "Application submitted successfully. Your credentials will be sent to your email once your application is approved.",
                applicationId = application.Id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submit an application via mentor invitation (public - no auth required).
    /// </summary>
    [HttpPost("apply/invited")]
    public async Task<IActionResult> ApplyInvited([FromForm] CreateStudentApplicationDto dto, IFormFile? schoolIdFile, [FromQuery] Guid mentorId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (mentorId == Guid.Empty)
            return BadRequest(new { message = "A valid mentor ID is required for invited applications." });

        try
        {
            // Upload school ID if provided
            string? schoolIdUrl = null;
            if (schoolIdFile != null && schoolIdFile.Length > 0)
            {
                using var stream = schoolIdFile.OpenReadStream();
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(schoolIdFile.FileName)}";
                schoolIdUrl = await _fileStorageService.UploadFileAsync(
                    stream, fileName, "student-applications/school-ids", schoolIdFile.ContentType);
            }

            var application = await _onboardingService.SubmitInvitedApplicationAsync(
                dto.FirstName, dto.LastName, dto.Email,
                dto.PhoneNumber ?? "", dto.StudentType, dto.UniversityName,
                dto.StudentIdNumber, dto.PortfolioUrl, schoolIdUrl, mentorId);

            return Ok(new
            {
                message = "Application submitted successfully. Your credentials will be sent to your email once your application is approved.",
                applicationId = application.Id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the authenticated student's profile.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return NotFound(new { message = "User not found." });

        var profile = await _onboardingService.GetStudentProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Student profile not found. Your application may still be pending." });

        // Get mentor name if assigned
        string? mentorName = null;
        if (profile.MentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(profile.MentorId.Value.ToString());
            if (mentor != null)
                mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }

        var dto = new StudentProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            StudentType = profile.StudentType,
            UniversityName = profile.UniversityName,
            EnrollmentStatus = profile.EnrollmentStatus,
            ExpectedGraduation = profile.ExpectedGraduation,
            StudentIdNumber = profile.StudentIdNumber,
            MentorId = profile.MentorId,
            MentorName = mentorName,
            MentorshipStatus = profile.MentorshipStatus,
            TotalProjectsCompleted = profile.TotalProjectsCompleted,
            AverageRating = profile.AverageRating,
            TotalEarnings = profile.TotalEarnings,
            CreatedAt = profile.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Update the authenticated student's profile.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _onboardingService.UpdateStudentProfileAsync(
            userId, dto.UniversityName, dto.EnrollmentStatus, dto.ExpectedGraduation);

        if (!result)
            return NotFound(new { message = "Student profile not found." });

        return Ok(new { message = "Profile updated successfully." });
    }

    /// <summary>
    /// Check if the authenticated user has a pending application.
    /// </summary>
    [Authorize]
    [HttpGet("application/status")]
    public async Task<IActionResult> GetApplicationStatus()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hasPending = await _onboardingService.HasPendingApplicationAsync(userId);

        return Ok(new { hasPendingApplication = hasPending });
    }

    // ────────────────────────────────────────
    // Student Project Endpoints
    // ────────────────────────────────────────

    /// <summary>
    /// List student's assigned projects.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var projects = await _mentorshipService.GetStudentProjectsAsync(userId);

        var dtos = new List<MentorshipProjectDto>();
        foreach (var p in projects)
        {
            string? mentorName = null;
            if (p.MentorId.HasValue)
            {
                var mentor = await _userManager.FindByIdAsync(p.MentorId.Value.ToString());
                if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
            }

            dtos.Add(new MentorshipProjectDto
            {
                Id = p.Id,
                ProjectNumber = p.ProjectNumber,
                Title = p.Title,
                Description = p.Description,
                Requirements = p.Requirements,
                ProjectType = p.ProjectType,
                Category = p.Category,
                Status = p.Status,
                Scope = p.Scope,
                EstimatedDeliveryDays = p.EstimatedDeliveryDays,
                ClientFee = p.ClientFee,
                MentorFee = p.MentorFee,
                StudentFee = p.StudentFee,
                Priority = p.Priority,
                MaxRevisions = p.MaxRevisions,
                CurrentRevisionCount = p.CurrentRevisionCount,
                MentorDeadline = p.MentorDeadline,
                StudentDeadline = p.StudentDeadline,
                CompletedAt = p.CompletedAt,
                MentorName = mentorName,
                PaymentStatus = p.PaymentStatus,
                CreatedAt = p.CreatedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific project's details (student view).
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("projects/{id}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);

        if (project == null || project.StudentId != userId)
            return NotFound(new { message = "Project not found." });

        string? mentorName = null;
        if (project.MentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(project.MentorId.Value.ToString());
            if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }

        return Ok(new MentorshipProjectDto
        {
            Id = project.Id,
            ProjectNumber = project.ProjectNumber,
            Title = project.Title,
            Description = project.Description,
            Requirements = project.Requirements,
            ProjectType = project.ProjectType,
            Category = project.Category,
            Status = project.Status,
            Scope = project.Scope,
            EstimatedDeliveryDays = project.EstimatedDeliveryDays,
            ClientFee = project.ClientFee,
            MentorFee = project.MentorFee,
            StudentFee = project.StudentFee,
            Priority = project.Priority,
            MaxRevisions = project.MaxRevisions,
            CurrentRevisionCount = project.CurrentRevisionCount,
            MentorDeadline = project.MentorDeadline,
            StudentDeadline = project.StudentDeadline,
            CompletedAt = project.CompletedAt,
            MentorName = mentorName,
            PaymentStatus = project.PaymentStatus,
            CreatedAt = project.CreatedAt
        });
    }

    /// <summary>
    /// Submit an iteration for mentor review.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPost("projects/{id}/submit")]
    public async Task<IActionResult> SubmitIteration(Guid id, [FromBody] SubmitIterationDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var iteration = await _mentorshipService.SubmitIterationAsync(id, userId, dto.Notes);
            return Ok(new
            {
                message = $"Iteration #{iteration.IterationNumber} submitted for review.",
                iterationId = iteration.Id,
                iterationNumber = iteration.IterationNumber
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// List iterations for a project (student view).
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("projects/{id}/iterations")]
    public async Task<IActionResult> GetIterations(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.StudentId != userId)
            return NotFound(new { message = "Project not found." });

        var iterations = await _mentorshipService.GetIterationsAsync(id);
        var dtos = new List<ProjectIterationDto>();

        foreach (var iter in iterations.OrderByDescending(i => i.IterationNumber))
        {
            var submitter = await _userManager.FindByIdAsync(iter.SubmittedById.ToString());
            string? reviewerName = null;
            if (iter.ReviewedById.HasValue)
            {
                var reviewer = await _userManager.FindByIdAsync(iter.ReviewedById.Value.ToString());
                if (reviewer != null) reviewerName = $"{reviewer.FirstName} {reviewer.LastName}";
            }

            dtos.Add(new ProjectIterationDto
            {
                Id = iter.Id,
                ProjectId = iter.ProjectId,
                IterationNumber = iter.IterationNumber,
                SubmittedById = iter.SubmittedById,
                SubmittedByName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}" : "",
                SubmittedByRole = iter.SubmittedByRole,
                Status = iter.Status,
                Notes = iter.Notes,
                ReviewNotes = iter.ReviewNotes,
                ReviewedById = iter.ReviewedById,
                ReviewedByName = reviewerName,
                ReviewedAt = iter.ReviewedAt,
                CreatedAt = iter.CreatedAt,
                Files = iter.Files.Select(f => new ProjectFileDto
                {
                    Id = f.Id,
                    IterationId = f.IterationId,
                    ProjectId = f.ProjectId,
                    FileName = f.FileName,
                    Category = f.Category,
                    StorageUrl = f.StorageUrl,
                    FileSizeBytes = f.FileSizeBytes,
                    Version = f.Version,
                    UploadedById = f.UploadedById,
                    CreatedAt = f.CreatedAt
                }).ToList()
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get messages for a project (student view).
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("projects/{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.StudentId != userId)
            return NotFound(new { message = "Project not found." });

        var messages = await _mentorshipService.GetMessagesAsync(id);
        var dtos = new List<ProjectMessageDto>();

        foreach (var msg in messages.OrderBy(m => m.CreatedAt))
        {
            var sender = await _userManager.FindByIdAsync(msg.SenderId.ToString());
            dtos.Add(new ProjectMessageDto
            {
                Id = msg.Id,
                ProjectId = msg.ProjectId,
                SenderId = msg.SenderId,
                SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "System",
                SenderRole = msg.SenderRole,
                Content = msg.Content,
                IsSystemMessage = msg.IsSystemMessage,
                CreatedAt = msg.CreatedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Send a message to the mentor for a project.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPost("projects/{id}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var message = await _mentorshipService.SendMessageAsync(id, userId, ProjectMessageSenderRole.Student, dto.Content);
            return Ok(new { message = "Message sent.", messageId = message.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ────────────────────────────────────────
    // Admin Endpoints
    // ────────────────────────────────────────

    /// <summary>
    /// Get all pending student applications (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/applications/pending")]
    public async Task<IActionResult> GetPendingApplications()
    {
        var applications = await _onboardingService.GetPendingApplicationsAsync();
        var dtos = new List<StudentApplicationDto>();

        foreach (var app in applications)
        {
            var user = await _userManager.FindByIdAsync(app.UserId.ToString());
            string? mentorName = null;
            if (app.InvitedByMentorId.HasValue)
            {
                var mentor = await _userManager.FindByIdAsync(app.InvitedByMentorId.Value.ToString());
                if (mentor != null)
                    mentorName = $"{mentor.FirstName} {mentor.LastName}";
            }

            dtos.Add(new StudentApplicationDto
            {
                Id = app.Id,
                UserId = app.UserId,
                FirstName = user?.FirstName ?? "",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                ApplicationType = app.ApplicationType,
                InvitedByMentorId = app.InvitedByMentorId,
                InvitedByMentorName = mentorName,
                StudentType = app.StudentType,
                UniversityName = app.UniversityName,
                StudentIdNumber = app.StudentIdNumber,
                SchoolIdUrl = app.SchoolIdUrl,
                PortfolioUrl = app.PortfolioUrl,
                Status = app.Status,
                ReviewNotes = app.ReviewNotes,
                ReviewedAt = app.ReviewedAt,
                CreatedAt = app.CreatedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get student applications by status (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/applications")]
    public async Task<IActionResult> GetApplicationsByStatus([FromQuery] ApplicationStatus? status)
    {
        IEnumerable<StudentApplication> applications;

        if (status.HasValue)
            applications = await _onboardingService.GetApplicationsByStatusAsync(status.Value);
        else
            applications = await _onboardingService.GetPendingApplicationsAsync();

        var dtos = new List<StudentApplicationDto>();

        foreach (var app in applications)
        {
            var user = await _userManager.FindByIdAsync(app.UserId.ToString());
            string? mentorName = null;
            if (app.InvitedByMentorId.HasValue)
            {
                var mentor = await _userManager.FindByIdAsync(app.InvitedByMentorId.Value.ToString());
                if (mentor != null)
                    mentorName = $"{mentor.FirstName} {mentor.LastName}";
            }

            dtos.Add(new StudentApplicationDto
            {
                Id = app.Id,
                UserId = app.UserId,
                FirstName = user?.FirstName ?? "",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                ApplicationType = app.ApplicationType,
                InvitedByMentorId = app.InvitedByMentorId,
                InvitedByMentorName = mentorName,
                StudentType = app.StudentType,
                UniversityName = app.UniversityName,
                StudentIdNumber = app.StudentIdNumber,
                SchoolIdUrl = app.SchoolIdUrl,
                PortfolioUrl = app.PortfolioUrl,
                Status = app.Status,
                ReviewNotes = app.ReviewNotes,
                ReviewedAt = app.ReviewedAt,
                CreatedAt = app.CreatedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Approve a student application (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/applications/{id}/approve")]
    public async Task<IActionResult> ApproveApplication(Guid id, [FromBody] ReviewStudentApplicationDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _onboardingService.ApproveApplicationAsync(id, adminId, dto.ReviewNotes);

        if (!result)
            return BadRequest(new { message = "Application not found or is not in a pending state." });

        return Ok(new { message = "Student application approved successfully." });
    }

    /// <summary>
    /// Reject a student application (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/applications/{id}/reject")]
    public async Task<IActionResult> RejectApplication(Guid id, [FromBody] ReviewStudentApplicationDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _onboardingService.RejectApplicationAsync(id, adminId, dto.ReviewNotes);

        if (!result)
            return BadRequest(new { message = "Application not found or is not in a pending state." });

        return Ok(new { message = "Student application rejected." });
    }

    /// <summary>
    /// Get a specific application by ID (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/applications/{id}")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var application = await _onboardingService.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound(new { message = "Application not found." });

        var user = await _userManager.FindByIdAsync(application.UserId.ToString());
        string? mentorName = null;
        if (application.InvitedByMentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(application.InvitedByMentorId.Value.ToString());
            if (mentor != null)
                mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }

        var dto = new StudentApplicationDto
        {
            Id = application.Id,
            UserId = application.UserId,
            FirstName = user?.FirstName ?? "",
            LastName = user?.LastName ?? "",
            Email = user?.Email ?? "",
            ApplicationType = application.ApplicationType,
            InvitedByMentorId = application.InvitedByMentorId,
            InvitedByMentorName = mentorName,
            StudentType = application.StudentType,
            UniversityName = application.UniversityName,
            StudentIdNumber = application.StudentIdNumber,
            SchoolIdUrl = application.SchoolIdUrl,
            PortfolioUrl = application.PortfolioUrl,
            Status = application.Status,
            ReviewNotes = application.ReviewNotes,
            ReviewedAt = application.ReviewedAt,
            CreatedAt = application.CreatedAt
        };

        return Ok(dto);
    }
}
