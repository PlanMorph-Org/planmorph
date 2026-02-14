using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MentorController : ControllerBase
{
    private readonly IMentorService _mentorService;
    private readonly IMentorshipService _mentorshipService;
    private readonly IStudentOnboardingService _studentService;
    private readonly UserManager<User> _userManager;

    public MentorController(
        IMentorService mentorService,
        IMentorshipService mentorshipService,
        IStudentOnboardingService studentService,
        UserManager<User> userManager)
    {
        _mentorService = mentorService;
        _mentorshipService = mentorshipService;
        _studentService = studentService;
        _userManager = userManager;
    }

    /// <summary>
    /// Check if the current user is eligible to become a mentor.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("eligibility")]
    public async Task<IActionResult> CheckEligibility()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isEligible = await _mentorService.IsMentorEligibleAsync(userId);

        // Check if already activated
        var existing = await _mentorService.GetMentorProfileAsync(userId);

        return Ok(new
        {
            isEligible,
            hasProfile = existing != null
        });
    }

    /// <summary>
    /// Activate a mentor profile for the current user.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("profile")]
    public async Task<IActionResult> ActivateProfile([FromBody] ActivateMentorProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var profile = await _mentorService.ActivateMentorProfileAsync(
                userId, dto.Bio, dto.Specializations,
                dto.MaxConcurrentStudents, dto.MaxConcurrentProjects);

            return Ok(new
            {
                message = "Mentor profile activated successfully.",
                profileId = profile.Id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get the current mentor's profile.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return NotFound(new { message = "User not found." });

        var profile = await _mentorService.GetMentorProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Mentor profile not found. Activate your mentor profile first." });

        var activeStudents = await _mentorService.GetActiveStudentCountAsync(userId);
        var activeProjects = await _mentorService.GetActiveProjectCountAsync(userId);

        var dto = new MentorProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            Specialization = user.Specialization,
            MaxConcurrentStudents = profile.MaxConcurrentStudents,
            MaxConcurrentProjects = profile.MaxConcurrentProjects,
            Bio = profile.Bio,
            Specializations = profile.Specializations,
            AcceptingStudents = profile.AcceptingStudents,
            TotalProjectsCompleted = profile.TotalProjectsCompleted,
            AverageRating = profile.AverageRating,
            TotalStudentsMentored = profile.TotalStudentsMentored,
            ActiveStudentCount = activeStudents,
            ActiveProjectCount = activeProjects,
            CreatedAt = profile.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Update the current mentor's profile.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateMentorProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorService.UpdateMentorProfileAsync(
            userId, dto.Bio, dto.Specializations,
            dto.MaxConcurrentStudents, dto.MaxConcurrentProjects,
            dto.AcceptingStudents);

        if (!result)
            return NotFound(new { message = "Mentor profile not found." });

        return Ok(new { message = "Profile updated successfully." });
    }

    /// <summary>
    /// Get mentor's dashboard statistics.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _mentorService.GetMentorProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Mentor profile not found." });

        var students = await _mentorService.GetMentorStudentsAsync(userId);
        var activeStudents = students.Count(s => s.Status == RelationshipStatus.Active);
        var activeProjects = await _mentorService.GetActiveProjectCountAsync(userId);

        var dto = new MentorStatsDto
        {
            TotalStudents = students.Count(),
            ActiveStudents = activeStudents,
            TotalProjectsCompleted = profile.TotalProjectsCompleted,
            ActiveProjects = activeProjects,
            AverageRating = profile.AverageRating
        };

        return Ok(dto);
    }

    // ────────────────────────────────────────
    // Student Management
    // ────────────────────────────────────────

    /// <summary>
    /// List the mentor's students.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _mentorService.GetMentorProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "Mentor profile not found." });

        var relationships = await _mentorService.GetMentorStudentsAsync(userId);
        var dtos = new List<MentorStudentDto>();

        foreach (var rel in relationships)
        {
            var student = await _userManager.FindByIdAsync(rel.StudentId.ToString());
            if (student == null) continue;

            var studentProfile = await _studentService.GetStudentProfileAsync(rel.StudentId);

            dtos.Add(new MentorStudentDto
            {
                RelationshipId = rel.Id,
                StudentUserId = rel.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email ?? "",
                StudentType = studentProfile?.StudentType.ToString() ?? "",
                UniversityName = studentProfile?.UniversityName ?? "",
                RelationshipStatus = rel.Status.ToString(),
                ProjectsCompleted = rel.ProjectsCompleted,
                StudentRating = rel.StudentRating,
                StartedAt = rel.StartedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Invite a student to the platform.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("students/invite")]
    public async Task<IActionResult> InviteStudent([FromBody] InviteStudentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _mentorService.InviteStudentAsync(
                userId, dto.Email, dto.FirstName, dto.LastName);

            if (!result)
                return BadRequest(new { message = "Failed to send invitation." });

            return Ok(new { message = $"Invitation sent to {dto.Email} successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a student from the mentor's roster.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpDelete("students/{studentUserId}")]
    public async Task<IActionResult> RemoveStudent(Guid studentUserId, [FromQuery] string? reason)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorService.RemoveStudentAsync(userId, studentUserId, reason);

        if (!result)
            return NotFound(new { message = "Student relationship not found." });

        return Ok(new { message = "Student removed successfully." });
    }

    // ────────────────────────────────────────
    // Project Management
    // ────────────────────────────────────────

    /// <summary>
    /// List published projects available for claiming.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("available-projects")]
    public async Task<IActionResult> GetAvailableProjects()
    {
        var projects = await _mentorshipService.GetPublishedProjectsAsync();

        var dtos = projects.Select(p => new MentorProjectDto
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
            ClientName = "",
            CreatedAt = p.CreatedAt
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Claim a published project.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("projects/{id}/claim")]
    public async Task<IActionResult> ClaimProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _mentorshipService.ClaimProjectAsync(id, userId);

            if (!result)
                return BadRequest(new { message = "Project cannot be claimed. It may not be in Published state." });

            return Ok(new { message = "Project claimed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// List mentor's own projects.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var projects = await _mentorshipService.GetMentorProjectsAsync(userId);

        var dtos = new List<MentorProjectDto>();
        foreach (var p in projects)
        {
            string? studentName = null;
            if (p.StudentId.HasValue)
            {
                var student = await _userManager.FindByIdAsync(p.StudentId.Value.ToString());
                if (student != null) studentName = $"{student.FirstName} {student.LastName}";
            }

            var client = await _userManager.FindByIdAsync(p.ClientId.ToString());

            dtos.Add(new MentorProjectDto
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
                ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "",
                StudentId = p.StudentId,
                StudentName = studentName,
                MentorDeadline = p.MentorDeadline,
                StudentDeadline = p.StudentDeadline,
                CreatedAt = p.CreatedAt
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific project's details.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("projects/{id}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);

        if (project == null || project.MentorId != userId)
            return NotFound(new { message = "Project not found." });

        string? studentName = null;
        if (project.StudentId.HasValue)
        {
            var student = await _userManager.FindByIdAsync(project.StudentId.Value.ToString());
            if (student != null) studentName = $"{student.FirstName} {student.LastName}";
        }

        var client = await _userManager.FindByIdAsync(project.ClientId.ToString());

        var dto = new MentorProjectDto
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
            ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "",
            StudentId = project.StudentId,
            StudentName = studentName,
            MentorDeadline = project.MentorDeadline,
            StudentDeadline = project.StudentDeadline,
            CreatedAt = project.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Assign a student to a claimed project.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("projects/{id}/assign-student")]
    public async Task<IActionResult> AssignStudent(Guid id, [FromBody] AssignStudentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _mentorshipService.AssignStudentAsync(id, userId, dto.StudentId);

            if (!result)
                return BadRequest(new { message = "Cannot assign student. The project must be in Claimed state and belong to you." });

            return Ok(new { message = "Student assigned successfully. Project is now in progress." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ────────────────────────────────────────
    // Iteration Review
    // ────────────────────────────────────────

    /// <summary>
    /// List iterations for a project (mentor view).
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("projects/{id}/iterations")]
    public async Task<IActionResult> GetIterations(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.MentorId != userId)
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
    /// Review a student's iteration (approve or request revision).
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("projects/{projectId}/iterations/{iterationId}/review")]
    public async Task<IActionResult> ReviewIteration(Guid projectId, Guid iterationId, [FromBody] ReviewIterationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.ReviewIterationAsync(iterationId, userId, dto.Approved, dto.ReviewNotes);

        if (!result)
            return BadRequest(new { message = "Cannot review this iteration. It may not exist, not belong to your project, or not be in Submitted state." });

        var action = dto.Approved ? "approved" : "sent back for revision";
        return Ok(new { message = $"Iteration {action} successfully." });
    }

    /// <summary>
    /// Deliver an approved iteration to the client.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("projects/{id}/deliver")]
    public async Task<IActionResult> DeliverToClient(Guid id, [FromBody] DeliverToClientDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.MentorId != userId)
            return NotFound(new { message = "Project not found." });

        // Find the latest approved iteration
        var iterations = await _mentorshipService.GetIterationsAsync(id);
        var approvedIteration = iterations
            .Where(i => i.Status == IterationStatus.Approved)
            .OrderByDescending(i => i.IterationNumber)
            .FirstOrDefault();

        if (approvedIteration == null)
            return BadRequest(new { message = "No approved iteration found to deliver." });

        try
        {
            var deliverable = await _mentorshipService.DeliverToClientAsync(id, userId, approvedIteration.Id, dto.MentorNotes);
            return Ok(new
            {
                message = $"Delivery #{deliverable.DeliveryNumber} sent to client for review.",
                deliverableId = deliverable.Id
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get messages for a project (mentor view).
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpGet("projects/{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.MentorId != userId)
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
    /// Send a message to the student for a project.
    /// </summary>
    [Authorize(Roles = "Architect,Engineer")]
    [HttpPost("projects/{id}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, [FromBody] SendMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var message = await _mentorshipService.SendMessageAsync(id, userId, ProjectMessageSenderRole.Mentor, dto.Content);
            return Ok(new { message = "Message sent.", messageId = message.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
