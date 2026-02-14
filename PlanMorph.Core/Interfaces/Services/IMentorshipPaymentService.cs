using PlanMorph.Core.Entities.Mentorship;

namespace PlanMorph.Core.Interfaces.Services;

public interface IMentorshipPaymentService
{
    // Client escrow payment
    Task<MentorshipPaymentResult> InitializeEscrowPaymentAsync(Guid projectId, Guid clientId);
    Task<bool> VerifyEscrowPaymentAsync(string reference);

    // Payment releases (admin-triggered)
    Task<bool> ReleaseMentorPaymentAsync(Guid projectId, Guid adminId);
    Task<bool> ReleaseStudentPaymentAsync(Guid projectId, Guid adminId);

    // Refunds
    Task<bool> ProcessRefundAsync(Guid projectId, Guid adminId, string? reason);

    // Payment disputes
    Task<bool> FreezePaymentAsync(Guid projectId, Guid adminId, string reason);
    Task<bool> UnfreezePaymentAsync(Guid projectId, Guid adminId, PaymentStatus resolvedStatus);

    // Queries
    Task<MentorshipPaymentDetailsDto?> GetPaymentDetailsAsync(Guid projectId);
    Task<IEnumerable<MentorshipPaymentDetailsDto>> GetPendingPayoutsAsync();
}

public class MentorshipPaymentResult
{
    public bool Success { get; set; }
    public string? AuthorizationUrl { get; set; }
    public string? Reference { get; set; }
    public string? AccessCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MentorshipPaymentDetailsDto
{
    public Guid ProjectId { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public ProjectStatus ProjectStatus { get; set; }
    public decimal ClientFee { get; set; }
    public decimal MentorFee { get; set; }
    public decimal StudentFee { get; set; }
    public decimal PlatformFee { get; set; }
    public string? PaymentReference { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? MentorId { get; set; }
    public string? MentorName { get; set; }
    public Guid? StudentId { get; set; }
    public string? StudentName { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
