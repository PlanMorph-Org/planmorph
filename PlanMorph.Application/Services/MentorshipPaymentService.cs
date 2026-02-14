using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PlanMorph.Application.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Entities.Mentorship;
using PlanMorph.Core.Interfaces;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Application.Services;

public class MentorshipPaymentService : IMentorshipPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaystackGateway _paystackGateway;
    private readonly IProjectWorkflowService _workflowService;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<MentorshipPaymentService> _logger;

    public MentorshipPaymentService(
        IUnitOfWork unitOfWork,
        IPaystackGateway paystackGateway,
        IProjectWorkflowService workflowService,
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<MentorshipPaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _paystackGateway = paystackGateway;
        _workflowService = workflowService;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    // ──────────────────────────────────────────
    // Client escrow payment
    // ──────────────────────────────────────────

    public async Task<MentorshipPaymentResult> InitializeEscrowPaymentAsync(Guid projectId, Guid clientId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null || project.ClientId != clientId)
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Project not found." };

        // Client can pay once the project is Scoped or Published (fees have been set)
        var payableStates = new HashSet<ProjectStatus>
        {
            ProjectStatus.Scoped, ProjectStatus.Published,
            ProjectStatus.Claimed, ProjectStatus.StudentAssigned
        };

        if (!payableStates.Contains(project.Status))
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Project is not in a payable state." };

        if (project.PaymentStatus != PaymentStatus.Pending)
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Payment has already been initiated or completed." };

        if (project.ClientFee <= 0)
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Project fee has not been set." };

        var client = await _userManager.FindByIdAsync(clientId.ToString());
        if (client?.Email == null)
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Client email not found." };

        // Use ProjectNumber as the unique Paystack reference
        var reference = project.ProjectNumber;

        var init = await _paystackGateway.InitializePaymentAsync(client.Email, project.ClientFee, reference);
        if (init == null)
            return new MentorshipPaymentResult { Success = false, ErrorMessage = "Unable to initialize payment with payment provider." };

        _logger.LogInformation("Escrow payment initialized for project {ProjectNumber}, amount {Amount}",
            project.ProjectNumber, project.ClientFee);

        return new MentorshipPaymentResult
        {
            Success = true,
            AuthorizationUrl = init.AuthorizationUrl,
            Reference = init.Reference,
            AccessCode = init.AccessCode
        };
    }

    public async Task<bool> VerifyEscrowPaymentAsync(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return false;

        var verify = await _paystackGateway.VerifyPaymentAsync(reference);
        if (verify == null || !verify.IsSuccessful)
            return false;

        // Find project by ProjectNumber (used as payment reference)
        var project = await _unitOfWork.MentorshipProjects
            .FirstOrDefaultAsync(p => p.ProjectNumber == reference);

        if (project == null)
            return false;

        // Already verified
        if (project.PaymentStatus == PaymentStatus.Escrowed)
            return true;

        // Verify amount matches (Paystack returns amount in kobo/cents)
        var expectedAmount = Convert.ToInt32(Math.Round(project.ClientFee * 100m, MidpointRounding.AwayFromZero));
        if (verify.AmountKobo != expectedAmount)
        {
            _logger.LogWarning("Payment amount mismatch for project {ProjectNumber}. Expected {Expected}, got {Actual}",
                project.ProjectNumber, expectedAmount, verify.AmountKobo);
            return false;
        }

        // Mark payment as escrowed
        project.PaymentStatus = PaymentStatus.Escrowed;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Audit log
        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = project.ClientId,
            Action = "PaymentEscrowed",
            NewValue = $"KES {project.ClientFee:N2} escrowed",
            Metadata = $"Reference: {reference}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Send confirmation email
        var client = await _userManager.FindByIdAsync(project.ClientId.ToString());
        if (client?.Email != null)
        {
            await _emailService.SendMentorshipPaymentConfirmationEmailAsync(
                client.Email,
                $"{client.FirstName} {client.LastName}",
                project.Title,
                project.ClientFee,
                project.ProjectNumber);
        }

        _logger.LogInformation("Escrow payment verified for project {ProjectNumber}, amount {Amount}",
            project.ProjectNumber, project.ClientFee);

        return true;
    }

    // ──────────────────────────────────────────
    // Payment releases
    // ──────────────────────────────────────────

    public async Task<bool> ReleaseMentorPaymentAsync(Guid projectId, Guid adminId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Must be completed and escrowed to release mentor payment
        if (project.Status != ProjectStatus.Completed && project.Status != ProjectStatus.Paid)
            return false;

        if (project.PaymentStatus != PaymentStatus.Escrowed)
            return false;

        if (!project.MentorId.HasValue)
            return false;

        // Update payment status
        project.PaymentStatus = PaymentStatus.MentorReleased;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Audit log
        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "MentorPaymentReleased",
            OldValue = PaymentStatus.Escrowed.ToString(),
            NewValue = $"KES {project.MentorFee:N2} released to mentor",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Notify mentor
        var mentor = await _userManager.FindByIdAsync(project.MentorId.Value.ToString());
        if (mentor?.Email != null)
        {
            await _emailService.SendMentorshipPaymentReleasedEmailAsync(
                mentor.Email,
                $"{mentor.FirstName} {mentor.LastName}",
                project.Title,
                project.MentorFee,
                "Mentor");
        }

        _logger.LogInformation("Mentor payment of {Amount} released for project {ProjectNumber}",
            project.MentorFee, project.ProjectNumber);

        return true;
    }

    public async Task<bool> ReleaseStudentPaymentAsync(Guid projectId, Guid adminId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Must have mentor payment released first
        if (project.PaymentStatus != PaymentStatus.MentorReleased)
            return false;

        if (!project.StudentId.HasValue)
            return false;

        // Update payment status
        project.PaymentStatus = PaymentStatus.StudentReleased;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Audit log
        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "StudentPaymentReleased",
            OldValue = PaymentStatus.MentorReleased.ToString(),
            NewValue = $"KES {project.StudentFee:N2} released to student",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Mark payment as fully completed
        project.PaymentStatus = PaymentStatus.Completed;
        project.PaidAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Transition project to Paid
        await _workflowService.TransitionAsync(projectId, ProjectStatus.Paid, adminId, "All payments released");

        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "PaymentCompleted",
            NewValue = "All payments released — project fully paid",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Notify student
        var student = await _userManager.FindByIdAsync(project.StudentId.Value.ToString());
        if (student?.Email != null)
        {
            await _emailService.SendMentorshipPaymentReleasedEmailAsync(
                student.Email,
                $"{student.FirstName} {student.LastName}",
                project.Title,
                project.StudentFee,
                "Student");
        }

        // Update mentor/student profiles
        await UpdateCompletionStatsAsync(project);

        _logger.LogInformation("Student payment of {Amount} released for project {ProjectNumber}. Project fully paid.",
            project.StudentFee, project.ProjectNumber);

        return true;
    }

    // ──────────────────────────────────────────
    // Refunds
    // ──────────────────────────────────────────

    public async Task<bool> ProcessRefundAsync(Guid projectId, Guid adminId, string? reason)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        // Can only refund escrowed payments
        if (project.PaymentStatus != PaymentStatus.Escrowed)
            return false;

        // Determine refund type based on project state
        var fullRefundStates = new HashSet<ProjectStatus>
        {
            ProjectStatus.Draft, ProjectStatus.Submitted,
            ProjectStatus.UnderReview, ProjectStatus.Scoped,
            ProjectStatus.Published, ProjectStatus.Cancelled
        };

        var isFullRefund = fullRefundStates.Contains(project.Status);
        var refundDescription = isFullRefund
            ? $"Full refund of KES {project.ClientFee:N2}"
            : $"Partial refund processed (work was in progress)";

        project.PaymentStatus = PaymentStatus.Refunded;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        // Audit log
        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "PaymentRefunded",
            OldValue = PaymentStatus.Escrowed.ToString(),
            NewValue = refundDescription,
            Metadata = reason,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Notify client
        var client = await _userManager.FindByIdAsync(project.ClientId.ToString());
        if (client?.Email != null)
        {
            await _emailService.SendMentorshipRefundEmailAsync(
                client.Email,
                $"{client.FirstName} {client.LastName}",
                project.Title,
                project.ClientFee,
                isFullRefund ? "Full" : "Partial",
                reason);
        }

        _logger.LogInformation("{RefundType} refund processed for project {ProjectNumber}. Reason: {Reason}",
            isFullRefund ? "Full" : "Partial", project.ProjectNumber, reason ?? "N/A");

        return true;
    }

    // ──────────────────────────────────────────
    // Dispute payment handling
    // ──────────────────────────────────────────

    public async Task<bool> FreezePaymentAsync(Guid projectId, Guid adminId, string reason)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        if (project.PaymentStatus != PaymentStatus.Escrowed)
            return false;

        var previousStatus = project.PaymentStatus;
        project.PaymentStatus = PaymentStatus.Disputed;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "PaymentFrozen",
            OldValue = previousStatus.ToString(),
            NewValue = PaymentStatus.Disputed.ToString(),
            Metadata = reason,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment frozen for project {ProjectNumber}. Reason: {Reason}",
            project.ProjectNumber, reason);

        return true;
    }

    public async Task<bool> UnfreezePaymentAsync(Guid projectId, Guid adminId, PaymentStatus resolvedStatus)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return false;

        if (project.PaymentStatus != PaymentStatus.Disputed)
            return false;

        // Can resolve to Escrowed (continue) or Refunded
        if (resolvedStatus != PaymentStatus.Escrowed && resolvedStatus != PaymentStatus.Refunded)
            return false;

        project.PaymentStatus = resolvedStatus;
        project.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.MentorshipProjects.UpdateAsync(project);

        await _unitOfWork.ProjectAuditLogs.AddAsync(new ProjectAuditLog
        {
            ProjectId = project.Id,
            ActorId = adminId,
            Action = "PaymentUnfrozen",
            OldValue = PaymentStatus.Disputed.ToString(),
            NewValue = resolvedStatus.ToString(),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment unfrozen for project {ProjectNumber}. Resolved to {Status}",
            project.ProjectNumber, resolvedStatus);

        return true;
    }

    // ──────────────────────────────────────────
    // Queries
    // ──────────────────────────────────────────

    public async Task<MentorshipPaymentDetailsDto?> GetPaymentDetailsAsync(Guid projectId)
    {
        var project = await _unitOfWork.MentorshipProjects.GetByIdAsync(projectId);
        if (project == null)
            return null;

        return await MapToPaymentDetailsAsync(project);
    }

    public async Task<IEnumerable<MentorshipPaymentDetailsDto>> GetPendingPayoutsAsync()
    {
        // Find projects where payment is escrowed and project is completed (ready for payout)
        var projects = await _unitOfWork.MentorshipProjects
            .FindAsync(p =>
                (p.PaymentStatus == PaymentStatus.Escrowed || p.PaymentStatus == PaymentStatus.MentorReleased)
                && (p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Paid));

        var dtos = new List<MentorshipPaymentDetailsDto>();
        foreach (var project in projects)
        {
            dtos.Add(await MapToPaymentDetailsAsync(project));
        }

        return dtos;
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private async Task<MentorshipPaymentDetailsDto> MapToPaymentDetailsAsync(MentorshipProject project)
    {
        var client = await _userManager.FindByIdAsync(project.ClientId.ToString());
        string? mentorName = null;
        string? studentName = null;

        if (project.MentorId.HasValue)
        {
            var mentor = await _userManager.FindByIdAsync(project.MentorId.Value.ToString());
            if (mentor != null) mentorName = $"{mentor.FirstName} {mentor.LastName}";
        }
        if (project.StudentId.HasValue)
        {
            var student = await _userManager.FindByIdAsync(project.StudentId.Value.ToString());
            if (student != null) studentName = $"{student.FirstName} {student.LastName}";
        }

        return new MentorshipPaymentDetailsDto
        {
            ProjectId = project.Id,
            ProjectNumber = project.ProjectNumber,
            Title = project.Title,
            PaymentStatus = project.PaymentStatus,
            ProjectStatus = project.Status,
            ClientFee = project.ClientFee,
            MentorFee = project.MentorFee,
            StudentFee = project.StudentFee,
            PlatformFee = project.PlatformFee,
            PaymentReference = project.ProjectNumber,
            ClientId = project.ClientId,
            ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "",
            MentorId = project.MentorId,
            MentorName = mentorName,
            StudentId = project.StudentId,
            StudentName = studentName,
            PaidAt = project.PaidAt,
            CreatedAt = project.CreatedAt
        };
    }

    private async Task UpdateCompletionStatsAsync(MentorshipProject project)
    {
        // Update mentor profile stats
        if (project.MentorId.HasValue)
        {
            var mentorProfile = await _unitOfWork.MentorProfiles
                .FirstOrDefaultAsync(p => p.UserId == project.MentorId.Value);
            if (mentorProfile != null)
            {
                mentorProfile.TotalProjectsCompleted++;
                mentorProfile.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.MentorProfiles.UpdateAsync(mentorProfile);
            }
        }

        // Update student profile stats
        if (project.StudentId.HasValue)
        {
            var studentProfile = await _unitOfWork.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == project.StudentId.Value);
            if (studentProfile != null)
            {
                studentProfile.TotalProjectsCompleted++;
                studentProfile.TotalEarnings += project.StudentFee;
                studentProfile.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.StudentProfiles.UpdateAsync(studentProfile);
            }
        }

        // Update mentor-student relationship
        if (project.MentorId.HasValue && project.StudentId.HasValue)
        {
            var relationship = await _unitOfWork.MentorStudentRelationships
                .FirstOrDefaultAsync(r => r.MentorId == project.MentorId.Value
                    && r.StudentId == project.StudentId.Value
                    && r.Status == RelationshipStatus.Active);
            if (relationship != null)
            {
                relationship.ProjectsCompleted++;
                await _unitOfWork.MentorStudentRelationships.UpdateAsync(relationship);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
