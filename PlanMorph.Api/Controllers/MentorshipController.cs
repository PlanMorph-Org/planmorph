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
[Authorize]
public class MentorshipController : ControllerBase
{
    private readonly IMentorshipService _mentorshipService;
    private readonly IProjectWorkflowService _workflowService;
    private readonly IMentorshipPaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public MentorshipController(
        IMentorshipService mentorshipService,
        IProjectWorkflowService workflowService,
        IMentorshipPaymentService paymentService,
        IUnitOfWork unitOfWork,
        UserManager<User> userManager)
    {
        _mentorshipService = mentorshipService;
        _workflowService = workflowService;
        _paymentService = paymentService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    // ────────────────────────────────────────
    // Client Endpoints
    // ────────────────────────────────────────

    /// <summary>
    /// Create a custom design request (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var project = await _mentorshipService.CreateProjectAsync(
                userId, dto.Title, dto.Description, dto.Requirements,
                dto.ProjectType, dto.Category, dto.Priority,
                dto.OrderId, dto.DesignId);

            return Ok(new
            {
                message = "Project request submitted successfully.",
                projectId = project.Id,
                projectNumber = project.ProjectNumber
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// List the client's mentorship projects.
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet("my-projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var projects = await _mentorshipService.GetClientProjectsAsync(userId);

        var dtos = new List<MentorshipProjectDto>();
        foreach (var p in projects)
        {
            string? mentorName = null;
            if (p.MentorId.HasValue)
            {
                var mentor = await _userManager.FindByIdAsync(p.MentorId.Value.ToString());
                if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
            }

            dtos.Add(MapToClientDto(p, mentorName));
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get project details (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);

        if (project == null || project.ClientId != userId)
            return NotFound(new { message = "Project not found." });

        string? mentorName = null;
        if (project.MentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(project.MentorId.Value.ToString());
            if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }

        return Ok(MapToClientDto(project, mentorName));
    }

    /// <summary>
    /// Cancel a project (Client — only before student assignment).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelProject(Guid id, [FromBody] CancelProjectDto? dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.CancelProjectAsync(id, userId, dto?.Reason);

        if (!result)
            return BadRequest(new { message = "Project cannot be cancelled. It may not exist, is not yours, or has progressed past the cancellable stage." });

        return Ok(new { message = "Project cancelled successfully." });
    }

    /// <summary>
    /// List deliverables for a project (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet("{id}/deliverables")]
    public async Task<IActionResult> GetDeliverables(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.ClientId != userId)
            return NotFound(new { message = "Project not found." });

        var deliverables = await _mentorshipService.GetDeliverablesAsync(id);
        var dtos = new List<ClientDeliverableDto>();

        foreach (var d in deliverables.OrderByDescending(d => d.DeliveryNumber))
        {
            var mentor = await _userManager.FindByIdAsync(d.DeliveredByMentorId.ToString());

            // Get files from the associated iteration
            var iteration = await _mentorshipService.GetIterationByIdAsync(d.IterationId);
            var files = iteration?.Files.Select(f => new ProjectFileDto
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
            }).ToList() ?? new List<ProjectFileDto>();

            dtos.Add(new ClientDeliverableDto
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                IterationId = d.IterationId,
                DeliveryNumber = d.DeliveryNumber,
                DeliveredByMentorId = d.DeliveredByMentorId,
                MentorName = mentor != null ? $"{mentor.FirstName} {mentor.LastName}" : "",
                Status = d.Status,
                ClientNotes = d.ClientNotes,
                MentorNotes = d.MentorNotes,
                DeliveredAt = d.DeliveredAt,
                ReviewedAt = d.ReviewedAt,
                Files = files
            });
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Accept a deliverable (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpPost("{id}/deliverables/{deliverableId}/accept")]
    public async Task<IActionResult> AcceptDeliverable(Guid id, Guid deliverableId, [FromBody] ReviewDeliverableDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.AcceptDeliverableAsync(deliverableId, userId, dto.ClientNotes);

        if (!result)
            return BadRequest(new { message = "Cannot accept deliverable. It may not exist, is not yours, or is not in Delivered state." });

        return Ok(new { message = "Deliverable accepted. Project is now complete." });
    }

    /// <summary>
    /// Request revision on a deliverable (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpPost("{id}/deliverables/{deliverableId}/revise")]
    public async Task<IActionResult> RequestRevision(Guid id, Guid deliverableId, [FromBody] ReviewDeliverableDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _mentorshipService.RequestDeliverableRevisionAsync(deliverableId, userId, dto.ClientNotes);

            if (!result)
                return BadRequest(new { message = "Cannot request revision. The deliverable may not exist or is not in Delivered state." });

            return Ok(new { message = "Revision requested. The mentor will coordinate updates." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Initialize escrow payment for a project (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpPost("{id}/pay")]
    public async Task<IActionResult> InitializePayment(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.InitializeEscrowPaymentAsync(id, userId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new
        {
            authorizationUrl = result.AuthorizationUrl,
            reference = result.Reference,
            accessCode = result.AccessCode
        });
    }

    /// <summary>
    /// Verify escrow payment (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet("{id}/pay/verify")]
    public async Task<IActionResult> VerifyPayment(Guid id, [FromQuery] string reference)
    {
        var verified = await _paymentService.VerifyEscrowPaymentAsync(reference);

        if (!verified)
            return BadRequest(new { message = "Payment verification failed." });

        return Ok(new { message = "Payment verified and escrowed successfully." });
    }

    /// <summary>
    /// Get payment details for a project (Client).
    /// </summary>
    [Authorize(Roles = "Client")]
    [HttpGet("{id}/payment")]
    public async Task<IActionResult> GetPaymentDetails(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null || project.ClientId != userId)
            return NotFound(new { message = "Project not found." });

        var details = await _paymentService.GetPaymentDetailsAsync(id);
        return Ok(details);
    }

    // ────────────────────────────────────────
    // Admin Endpoints
    // ────────────────────────────────────────

    /// <summary>
    /// List all mentorship projects (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/projects")]
    public async Task<IActionResult> AdminGetProjects([FromQuery] ProjectStatus? status)
    {
        IEnumerable<MentorshipProject> projects;

        if (status.HasValue)
            projects = await _mentorshipService.GetProjectsByStatusAsync(status.Value);
        else
            projects = await _mentorshipService.GetAllProjectsAsync();

        var dtos = new List<AdminProjectDto>();
        foreach (var p in projects)
        {
            dtos.Add(await MapToAdminDto(p));
        }

        return Ok(dtos);
    }

    /// <summary>
    /// Get a specific project with full details (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/projects/{id}")]
    public async Task<IActionResult> AdminGetProject(Guid id)
    {
        var project = await _mentorshipService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound(new { message = "Project not found." });

        return Ok(await MapToAdminDto(project));
    }

    /// <summary>
    /// Scope a project — set fees, delivery estimate, max revisions (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/projects/{id}/scope")]
    public async Task<IActionResult> ScopeProject(Guid id, [FromBody] ScopeProjectDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.ScopeProjectAsync(
            id, adminId, dto.Scope, dto.ClientFee,
            dto.MentorFee, dto.StudentFee,
            dto.EstimatedDeliveryDays, dto.MaxRevisions);

        if (!result)
            return BadRequest(new { message = "Project cannot be scoped. It may not exist or is not in the correct state." });

        return Ok(new { message = "Project scoped successfully." });
    }

    /// <summary>
    /// Publish a scoped project for mentors to claim (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/projects/{id}/publish")]
    public async Task<IActionResult> PublishProject(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.PublishProjectAsync(id, adminId);

        if (!result)
            return BadRequest(new { message = "Project cannot be published. It must be in Scoped state." });

        return Ok(new { message = "Project published and available for mentors." });
    }

    /// <summary>
    /// Manually assign a mentor to a project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/projects/{id}/assign-mentor")]
    public async Task<IActionResult> AssignMentor(Guid id, [FromBody] AssignMentorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _mentorshipService.AssignMentorAsync(id, adminId, dto.MentorId);

        if (!result)
            return BadRequest(new { message = "Cannot assign mentor. The project must be Published and the mentor must have an active profile." });

        return Ok(new { message = "Mentor assigned successfully." });
    }

    /// <summary>
    /// Get overall mentorship statistics (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalProjects = await _unitOfWork.MentorshipProjects.CountAsync();
        var activeProjects = await _unitOfWork.MentorshipProjects
            .CountAsync(p => p.Status != ProjectStatus.Completed && p.Status != ProjectStatus.Paid
                && p.Status != ProjectStatus.Cancelled);
        var completedProjects = await _unitOfWork.MentorshipProjects
            .CountAsync(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Paid);
        var pendingReview = await _unitOfWork.MentorshipProjects
            .CountAsync(p => p.Status == ProjectStatus.Submitted || p.Status == ProjectStatus.UnderReview);
        var totalStudents = await _unitOfWork.StudentProfiles.CountAsync();
        var totalMentors = await _unitOfWork.MentorProfiles.CountAsync();

        var completedProjectsList = await _unitOfWork.MentorshipProjects
            .FindAsync(p => p.Status == ProjectStatus.Paid);
        var totalRevenue = completedProjectsList.Sum(p => p.PlatformFee);

        var dto = new MentorshipStatsDto
        {
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            CompletedProjects = completedProjects,
            PendingReviewProjects = pendingReview,
            TotalStudents = totalStudents,
            TotalMentors = totalMentors,
            TotalRevenue = totalRevenue
        };

        return Ok(dto);
    }

    /// <summary>
    /// Get audit log for a project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/audit-log/{projectId}")]
    public async Task<IActionResult> GetAuditLog(Guid projectId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Project not found." });

        var logs = await _unitOfWork.ProjectAuditLogs
            .FindAsync(l => l.ProjectId == projectId);

        var logDtos = new List<object>();
        foreach (var log in logs.OrderByDescending(l => l.CreatedAt))
        {
            string? actorName = null;
            var actor = await _userManager.FindByIdAsync(log.ActorId.ToString());
            if (actor != null) actorName = $"{actor.FirstName} {actor.LastName}";

            logDtos.Add(new
            {
                log.Id,
                log.ProjectId,
                log.ActorId,
                ActorName = actorName,
                log.Action,
                log.OldValue,
                log.NewValue,
                log.Metadata,
                log.IpAddress,
                log.CreatedAt
            });
        }

        return Ok(logDtos);
    }

    /// <summary>
    /// Get payment details for a project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/projects/{id}/payment")]
    public async Task<IActionResult> AdminGetPaymentDetails(Guid id)
    {
        var details = await _paymentService.GetPaymentDetailsAsync(id);
        if (details == null)
            return NotFound(new { message = "Project not found." });

        return Ok(details);
    }

    /// <summary>
    /// List projects with pending payouts (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/pending-payouts")]
    public async Task<IActionResult> GetPendingPayouts()
    {
        var payouts = await _paymentService.GetPendingPayoutsAsync();
        return Ok(payouts);
    }

    /// <summary>
    /// Release mentor payment for a completed project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/projects/{id}/release-mentor-payment")]
    public async Task<IActionResult> ReleaseMentorPayment(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.ReleaseMentorPaymentAsync(id, adminId);

        if (!result)
            return BadRequest(new { message = "Cannot release mentor payment. The project must be completed and payment must be escrowed." });

        return Ok(new { message = "Mentor payment released successfully." });
    }

    /// <summary>
    /// Release student payment for a completed project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/projects/{id}/release-student-payment")]
    public async Task<IActionResult> ReleaseStudentPayment(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.ReleaseStudentPaymentAsync(id, adminId);

        if (!result)
            return BadRequest(new { message = "Cannot release student payment. Mentor payment must be released first." });

        return Ok(new { message = "Student payment released. Project is now fully paid." });
    }

    /// <summary>
    /// Process a refund for a project (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/projects/{id}/refund")]
    public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] ProcessRefundDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.ProcessRefundAsync(id, adminId, dto.Reason);

        if (!result)
            return BadRequest(new { message = "Cannot process refund. Payment must be in escrowed state." });

        return Ok(new { message = "Refund processed successfully." });
    }

    /// <summary>
    /// Freeze payment due to a dispute (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/projects/{id}/freeze-payment")]
    public async Task<IActionResult> FreezePayment(Guid id, [FromBody] FreezePaymentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.FreezePaymentAsync(id, adminId, dto.Reason);

        if (!result)
            return BadRequest(new { message = "Cannot freeze payment. It must be in escrowed state." });

        return Ok(new { message = "Payment frozen pending dispute resolution." });
    }

    /// <summary>
    /// Unfreeze payment after dispute resolution (Admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/projects/{id}/unfreeze-payment")]
    public async Task<IActionResult> UnfreezePayment(Guid id, [FromBody] UnfreezePaymentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _paymentService.UnfreezePaymentAsync(id, adminId, dto.ResolvedStatus);

        if (!result)
            return BadRequest(new { message = "Cannot unfreeze payment. It must be in disputed state, and resolved status must be Escrowed or Refunded." });

        return Ok(new { message = $"Payment unfrozen. Status set to {dto.ResolvedStatus}." });
    }

    // ────────────────────────────────────────
    // Mapping helpers
    // ────────────────────────────────────────

    private static MentorshipProjectDto MapToClientDto(MentorshipProject p, string? mentorName)
    {
        return new MentorshipProjectDto
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
            Priority = p.Priority,
            MaxRevisions = p.MaxRevisions,
            CurrentRevisionCount = p.CurrentRevisionCount,
            MentorDeadline = p.MentorDeadline,
            StudentDeadline = p.StudentDeadline,
            CompletedAt = p.CompletedAt,
            MentorName = mentorName,
            PaymentStatus = p.PaymentStatus,
            CreatedAt = p.CreatedAt
        };
    }

    private async Task<AdminProjectDto> MapToAdminDto(MentorshipProject p)
    {
        var client = await _userManager.FindByIdAsync(p.ClientId.ToString());
        string? mentorName = null;
        string? studentName = null;

        if (p.MentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(p.MentorId.Value.ToString());
            if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }
        if (p.StudentId.HasValue)
        {
            var student = await _userManager.FindByIdAsync(p.StudentId.Value.ToString());
            if (student != null) studentName = $"{student.FirstName} {student.LastName}";
        }

        return new AdminProjectDto
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
            PlatformFee = p.PlatformFee,
            PaymentStatus = p.PaymentStatus,
            Priority = p.Priority,
            MaxRevisions = p.MaxRevisions,
            CurrentRevisionCount = p.CurrentRevisionCount,
            ClientId = p.ClientId,
            ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "",
            MentorId = p.MentorId,
            MentorName = mentorName,
            StudentId = p.StudentId,
            StudentName = studentName,
            MentorDeadline = p.MentorDeadline,
            StudentDeadline = p.StudentDeadline,
            CompletedAt = p.CompletedAt,
            PaidAt = p.PaidAt,
            CancelledAt = p.CancelledAt,
            CancellationReason = p.CancellationReason,
            CreatedAt = p.CreatedAt
        };
    }
}

public record CancelProjectDto
{
    public string? Reason { get; init; }
}

public record ProcessRefundDto
{
    public string? Reason { get; init; }
}

public record FreezePaymentDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(1000, MinimumLength = 1)]
    public string Reason { get; init; } = string.Empty;
}

public record UnfreezePaymentDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public PaymentStatus ResolvedStatus { get; init; }
}
