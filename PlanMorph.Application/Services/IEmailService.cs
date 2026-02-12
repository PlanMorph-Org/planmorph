namespace PlanMorph.Application.Services;

public interface IEmailService
{
    Task SendDesignApprovedEmailAsync(string toEmail, string architectName, string designTitle);
    Task SendDesignRejectedEmailAsync(string toEmail, string architectName, string designTitle, string? reason = null);
    Task SendArchitectApprovedEmailAsync(string toEmail, string architectName);
    Task SendArchitectRejectedEmailAsync(string toEmail, string architectName, string? reason = null);
    Task SendOrderConfirmationEmailAsync(string toEmail, string customerName, string designTitle, decimal amount);
    Task SendWelcomeEmailAsync(string toEmail, string firstName);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken);

    // Construction Service Emails
    Task SendConstructionRequestReceivedEmailAsync(string toEmail, string clientName, string designTitle, string location);
    Task SendAdminConstructionRequestNotificationAsync(string adminEmail, string clientName, string designTitle, string location, string orderNumber);
    Task SendContractorAssignedToClientEmailAsync(string toEmail, string clientName, string designTitle, string contractorName, string location);
    Task SendContractorAssignmentEmailAsync(string toEmail, string contractorName, string designTitle, string clientName, string location, decimal estimatedCost);
}
