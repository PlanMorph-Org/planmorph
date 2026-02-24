namespace PlanMorph.Application.Services;

public interface IEmailService
{
    Task SendDesignApprovedEmailAsync(string toEmail, string architectName, string designTitle);
    Task SendDesignRejectedEmailAsync(string toEmail, string architectName, string designTitle, string? reason = null);
    Task SendArchitectApprovedEmailAsync(string toEmail, string architectName);
    Task SendArchitectRejectedEmailAsync(string toEmail, string architectName, string? reason = null);
    Task SendOrderConfirmationEmailAsync(string toEmail, string customerName, string designTitle, decimal amount);
    Task SendWelcomeEmailAsync(string toEmail, string firstName);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetUrl, bool isProfessional = false, string? verificationCode = null);

    // Construction Service Emails
    Task SendConstructionRequestReceivedEmailAsync(string toEmail, string clientName, string designTitle, string location);
    Task SendAdminConstructionRequestNotificationAsync(string adminEmail, string clientName, string designTitle, string location, string orderNumber);
    Task SendContractorAssignedToClientEmailAsync(string toEmail, string clientName, string designTitle, string contractorName, string location);
    Task SendContractorAssignmentEmailAsync(string toEmail, string contractorName, string designTitle, string clientName, string location, decimal estimatedCost);

    // Support Ticket Emails
    Task SendTicketCreatedEmailAsync(string toEmail, string subject, string ticketId, string priority, string category);
    Task SendNewTicketAlertEmailAsync(string ticketId, string subject, string clientId, string priority, string category);
    Task SendTicketStatusChangedEmailAsync(string toEmail, string ticketId, string subject, string previousStatus, string newStatus);
    Task SendTicketAssignedEmailAsync(string toEmail, string ticketId, string subject, string adminName);
    Task SendTicketReplyEmailAsync(string toEmail, string ticketId, string subject, string messageContent, string replyFromName);
    Task SendTicketUpdatedEmailAsync(string ticketId, string subject, string clientId, string messageContent);
    Task SendTicketClosedEmailAsync(string toEmail, string ticketId, string subject);
    Task SendTicketReopenedEmailAsync(string toEmail, string ticketId, string subject);

    // Student Mentorship Emails
    Task SendStudentApplicationReceivedEmailAsync(string toEmail, string studentName);
    Task SendAdminNewStudentApplicationEmailAsync(string studentName, string studentType, string universityName);
    Task SendStudentApplicationApprovedEmailAsync(string toEmail, string studentName);
    Task SendStudentApplicationRejectedEmailAsync(string toEmail, string studentName, string? reason = null);
    Task SendStudentCredentialsEmailAsync(string toEmail, string studentName, string temporaryPassword);
    Task SendMentorStudentInvitationEmailAsync(string toEmail, string studentName, string mentorName);

    // Mentorship Payment Emails
    Task SendMentorshipPaymentConfirmationEmailAsync(string toEmail, string clientName, string projectTitle, decimal amount, string projectNumber);
    Task SendMentorshipPaymentReleasedEmailAsync(string toEmail, string recipientName, string projectTitle, decimal amount, string role);
    Task SendMentorshipRefundEmailAsync(string toEmail, string clientName, string projectTitle, decimal amount, string refundType, string? reason);
}
