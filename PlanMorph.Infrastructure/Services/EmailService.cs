using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlanMorph.Application.Services;

namespace PlanMorph.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _smtpHost = NormalizeValue(_configuration["EmailSettings:SmtpHost"], "smtp.gmail.com");

        var smtpPortRaw = NormalizeValue(_configuration["EmailSettings:SmtpPort"], "587");
        if (!int.TryParse(smtpPortRaw, out _smtpPort))
        {
            _logger.LogWarning(
                "Invalid EmailSettings:SmtpPort value '{SmtpPort}'. Falling back to 587.",
                smtpPortRaw);
            _smtpPort = 587;
        }

        _smtpUsername = NormalizeValue(_configuration["EmailSettings:SmtpUsername"], "");
        _smtpPassword = NormalizeValue(_configuration["EmailSettings:SmtpPassword"], "");
        _fromEmail = NormalizeValue(_configuration["EmailSettings:FromEmail"], "noreply@planmorph.com");
        _fromName = NormalizeValue(_configuration["EmailSettings:FromName"], "PlanMorph");
    }

    private static string NormalizeValue(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var trimmed = value.Trim();
        if (string.Equals(trimmed, "<set-in-env>", StringComparison.OrdinalIgnoreCase))
            return fallback;

        // Protect against unresolved app-spec placeholders like `${SMTP_PORT}`.
        if (trimmed.StartsWith("${", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
            return fallback;

        return trimmed;
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogWarning("Email service not configured. Email not sent to {Email} with subject: {Subject}", toEmail, subject);
                return;
            }

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject: {Subject}", toEmail, subject);
            // Don't throw - email failure shouldn't break the application flow
        }
    }

    public async Task SendDesignApprovedEmailAsync(string toEmail, string architectName, string designTitle)
    {
        var subject = "Your Design Has Been Approved! 🎉";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #10b981; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Congratulations!</h1>
        </div>
        <div class=""content"">
            <p>Hi {architectName},</p>

            <p>Great news! Your design <strong>{designTitle}</strong> has been approved and is now live on the PlanMorph marketplace.</p>

            <p>Your design is now visible to all potential buyers. You'll receive notifications when someone purchases your design.</p>

            <p><strong>What happens next?</strong></p>
            <ul>
                <li>Your design is now searchable and can be purchased by clients</li>
                <li>You'll earn 70% commission on each sale</li>
                <li>Track your sales and earnings in your architect dashboard</li>
            </ul>

            <a href=""https://planmorph.com/architect/dashboard"" class=""button"">View Your Dashboard</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendDesignRejectedEmailAsync(string toEmail, string architectName, string designTitle, string? reason = null)
    {
        var subject = "Update on Your Design Submission";
        var reasonText = !string.IsNullOrEmpty(reason)
            ? $"<p><strong>Reason:</strong> {reason}</p>"
            : "";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Design Submission Update</h1>
        </div>
        <div class=""content"">
            <p>Hi {architectName},</p>

            <p>Thank you for submitting your design <strong>{designTitle}</strong> to PlanMorph.</p>

            <p>Unfortunately, we're unable to approve this design at this time.</p>

            {reasonText}

            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>Review our design guidelines</li>
                <li>Make necessary improvements to your design</li>
                <li>Resubmit your design for review</li>
            </ul>

            <p>If you have questions about this decision, please contact our support team.</p>

            <a href=""https://planmorph.com/architect/dashboard"" class=""button"">Go to Dashboard</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendArchitectApprovedEmailAsync(string toEmail, string architectName)
    {
        var subject = "Welcome to PlanMorph - Your Architect Account is Approved! 🎉";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #10b981; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to PlanMorph!</h1>
        </div>
        <div class=""content"">
            <p>Hi {architectName},</p>

            <p>Congratulations! Your architect account has been approved. You can now start uploading and selling your architectural designs on PlanMorph.</p>

            <p><strong>Getting Started:</strong></p>
            <ul>
                <li>Log in to your architect dashboard</li>
                <li>Upload your first design</li>
                <li>Add high-quality preview images and files</li>
                <li>Set your pricing (you earn 70% commission on each sale)</li>
            </ul>

            <p><strong>What Makes a Great Design Listing:</strong></p>
            <ul>
                <li>Clear, professional preview images</li>
                <li>Accurate specifications (bedrooms, bathrooms, size)</li>
                <li>Complete file sets (architectural drawings, structural drawings, BOQ)</li>
                <li>Competitive pricing</li>
            </ul>

            <a href=""https://planmorph.com/architect/login"" class=""button"">Go to Dashboard</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendArchitectRejectedEmailAsync(string toEmail, string architectName, string? reason = null)
    {
        var subject = "Update on Your Architect Application";
        var reasonText = !string.IsNullOrEmpty(reason)
            ? $"<p><strong>Reason:</strong> {reason}</p>"
            : "";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Update</h1>
        </div>
        <div class=""content"">
            <p>Hi {architectName},</p>

            <p>Thank you for your interest in joining PlanMorph as an architect.</p>

            <p>After careful review, we're unable to approve your architect account at this time.</p>

            {reasonText}

            <p>If you believe this is an error or would like to discuss your application, please contact our support team.</p>

            <a href=""https://planmorph.com/contact"" class=""button"">Contact Support</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendOrderConfirmationEmailAsync(string toEmail, string customerName, string designTitle, decimal amount)
    {
        var subject = "Order Confirmation - Your Architectural Plans Are Ready!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .order-details {{ background-color: white; padding: 20px; border-radius: 6px; margin: 20px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Order Confirmed!</h1>
        </div>
        <div class=""content"">
            <p>Hi {customerName},</p>

            <p>Thank you for your purchase! Your architectural plans are now ready to download.</p>

            <div class=""order-details"">
                <h3>Order Details</h3>
                <p><strong>Design:</strong> {designTitle}</p>
                <p><strong>Amount Paid:</strong> KES {amount:N2}</p>
            </div>

            <p><strong>Your Purchase Includes:</strong></p>
            <ul>
                <li>Complete architectural drawings</li>
                <li>Structural drawings</li>
                <li>Bill of Quantities (BOQ)</li>
                <li>High-resolution renders</li>
                <li>CAD files (DWG format)</li>
            </ul>

            <p>You can download your files anytime from your account.</p>

            <a href=""https://planmorph.com/my-orders"" class=""button"">Download Your Files</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName)
    {
        var subject = "Welcome to PlanMorph!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to PlanMorph!</h1>
        </div>
        <div class=""content"">
            <p>Hi {firstName},</p>

            <p>Welcome to PlanMorph - a global marketplace for architectural designs!</p>

            <p><strong>What You Can Do:</strong></p>
            <ul>
                <li>Browse hundreds of professionally designed house plans</li>
                <li>Filter by bedrooms, bathrooms, size, and budget</li>
                <li>Purchase and instantly download complete architectural plans</li>
                <li>Request construction services from verified contractors</li>
                <li>Request modifications to existing designs</li>
            </ul>

            <p><strong>Construction services:</strong> Currently available in Kenya only.</p>

            <p>Start exploring our collection of designs and find the perfect plan for your dream home!</p>

            <a href=""https://planmorph.com/designs"" class=""button"">Browse Designs</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken)
    {
        var resetUrl = $"https://planmorph.com/reset-password?token={resetToken}";
        var subject = "Reset Your Password - PlanMorph";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hi {firstName},</p>

            <p>We received a request to reset your password for your PlanMorph account.</p>

            <p>Click the button below to reset your password:</p>

            <a href=""{resetUrl}"" class=""button"">Reset Password</a>

            <div class=""warning"">
                <strong>Security Note:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email or contact support if you have concerns.
            </div>

            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #3b82f6;"">{resetUrl}</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendConstructionRequestReceivedEmailAsync(string toEmail, string clientName, string designTitle, string location)
    {
        var subject = "Construction Service Request Received - PlanMorph";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .info-box {{ background-color: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #10b981; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #10b981; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🏗️ Construction Request Received</h1>
        </div>
        <div class=""content"">
            <p>Hi {clientName},</p>

            <p>Thank you for requesting construction services through PlanMorph! We've received your request and are processing it.</p>

            <div class=""info-box"">
                <h3>Request Details</h3>
                <p><strong>Design:</strong> {designTitle}</p>
                <p><strong>Construction Location:</strong> {location}</p>
                <p><strong>Service Area:</strong> Kenya</p>
            </div>

            <p><strong>What Happens Next:</strong></p>
            <ol>
                <li>Our team will review your construction request</li>
                <li>We'll match you with a verified, qualified contractor in your area</li>
                <li>The contractor will contact you to discuss project details and timeline</li>
                <li>You'll receive an email notification once a contractor is assigned</li>
            </ol>

            <p><strong>Timeline:</strong> Contractor assignment typically takes 2-3 business days.</p>

            <p><strong>Platform Commission:</strong> A 2% platform fee applies to construction contracts to maintain our service quality and support.</p>

            <p>If you have any questions or need to update your request, please contact our support team.</p>

            <a href=""https://planmorph.com/my-orders"" class=""button"">View My Orders</a>
        </div>
        <div class=""footer"">
            <p>Questions? Email us at construction@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendAdminConstructionRequestNotificationAsync(string adminEmail, string clientName, string designTitle, string location, string orderNumber)
    {
        var subject = $"New Construction Request - {orderNumber}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f59e0b; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .info-box {{ background-color: white; padding: 20px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #f59e0b; }}
        .alert {{ background-color: #fef3c7; padding: 15px; border-radius: 6px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #f59e0b; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>⚡ Action Required: New Construction Request</h1>
        </div>
        <div class=""content"">
            <div class=""alert"">
                <strong>🔔 Admin Action Needed:</strong> A client has requested construction services and requires contractor assignment.
            </div>

            <div class=""info-box"">
                <h3>Construction Request Details</h3>
                <p><strong>Order Number:</strong> {orderNumber}</p>
                <p><strong>Client Name:</strong> {clientName}</p>
                <p><strong>Design:</strong> {designTitle}</p>
                <p><strong>Construction Location:</strong> {location}</p>
                <p><strong>Status:</strong> Pending Contractor Assignment</p>
            </div>

            <p><strong>Required Actions:</strong></p>
            <ol>
                <li>Review the order and design details in the admin panel</li>
                <li>Verify the construction location is within serviceable area (Kenya)</li>
                <li>Select and assign a qualified contractor from the contractor database</li>
                <li>Ensure contractor availability and location match</li>
                <li>Notify client once contractor is assigned</li>
            </ol>

            <p><strong>Assignment Criteria:</strong></p>
            <ul>
                <li>Contractor location proximity to project site</li>
                <li>Contractor experience with similar projects</li>
                <li>Current workload and availability</li>
                <li>Past performance and client ratings</li>
            </ul>

            <a href=""https://planmorph.com/admin/orders"" class=""button"">Assign Contractor Now</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Admin Notification System</p>
            <p>&copy; 2024 PlanMorph.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(adminEmail, subject, htmlBody);
    }

    public async Task SendContractorAssignedToClientEmailAsync(string toEmail, string clientName, string designTitle, string contractorName, string location)
    {
        var subject = "Contractor Assigned to Your Project - PlanMorph";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .contractor-card {{ background-color: white; padding: 25px; border-radius: 8px; margin: 20px 0; border: 2px solid #10b981; }}
        .info-box {{ background-color: #ecfdf5; padding: 15px; border-radius: 6px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #10b981; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ Contractor Assigned!</h1>
        </div>
        <div class=""content"">
            <p>Hi {clientName},</p>

            <p>Great news! We've assigned a qualified contractor to your construction project.</p>

            <div class=""contractor-card"">
                <h3>Your Assigned Contractor</h3>
                <p><strong>Contractor:</strong> {contractorName}</p>
                <p><strong>Project Design:</strong> {designTitle}</p>
                <p><strong>Location:</strong> {location}</p>
            </div>

            <div class=""info-box"">
                <strong>📞 Next Steps:</strong>
                <p>Your contractor will contact you within 24-48 hours to:</p>
                <ul>
                    <li>Introduce themselves and discuss their experience</li>
                    <li>Schedule a site visit and project assessment</li>
                    <li>Provide a detailed quote and timeline</li>
                    <li>Answer any questions about the construction process</li>
                    <li>Discuss payment terms and milestones</li>
                </ul>
            </div>

            <p><strong>Before Your First Meeting:</strong></p>
            <ul>
                <li>Review your downloaded design files</li>
                <li>Prepare a list of questions and specific requirements</li>
                <li>Consider your budget and timeline expectations</li>
                <li>Check local building permits and requirements</li>
            </ul>

            <p><strong>Important Reminders:</strong></p>
            <ul>
                <li>All contracts are between you and the contractor</li>
                <li>Verify contractor credentials and insurance</li>
                <li>Get written quotes and agreements</li>
                <li>Request progress updates and milestone confirmations</li>
                <li>PlanMorph is here to support you throughout the process</li>
            </ul>

            <p>If you have any concerns or need additional support, please don't hesitate to contact us.</p>

            <a href=""https://planmorph.com/my-orders"" class=""button"">View Project Details</a>
        </div>
        <div class=""footer"">
            <p>Need help? Contact us at construction@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendContractorAssignmentEmailAsync(string toEmail, string contractorName, string designTitle, string clientName, string location, decimal estimatedCost)
    {
        var subject = "New Construction Project Assignment - PlanMorph";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .project-card {{ background-color: white; padding: 25px; border-radius: 8px; margin: 20px 0; border: 2px solid #3b82f6; }}
        .highlight {{ background-color: #dbeafe; padding: 15px; border-radius: 6px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 New Project Assignment</h1>
        </div>
        <div class=""content"">
            <p>Hi {contractorName},</p>

            <p>Congratulations! You've been assigned a new construction project through PlanMorph.</p>

            <div class=""project-card"">
                <h3>Project Details</h3>
                <p><strong>Client Name:</strong> {clientName}</p>
                <p><strong>Design:</strong> {designTitle}</p>
                <p><strong>Construction Location:</strong> {location}</p>
                <p><strong>Estimated Project Cost:</strong> KES {estimatedCost:N2}</p>
                <p><strong>Platform Commission:</strong> 2% (KES {(estimatedCost * 0.02m):N2})</p>
            </div>

            <div class=""highlight"">
                <strong>⏰ Action Required:</strong>
                <p>Please contact the client within <strong>24-48 hours</strong> to discuss the project and schedule a site visit.</p>
            </div>

            <p><strong>Next Steps:</strong></p>
            <ol>
                <li><strong>Contact Client:</strong> Reach out via phone or email to introduce yourself</li>
                <li><strong>Review Design:</strong> Thoroughly examine the architectural plans and specifications</li>
                <li><strong>Site Visit:</strong> Schedule and conduct a comprehensive site assessment</li>
                <li><strong>Detailed Quote:</strong> Provide an itemized quote including materials, labor, and timeline</li>
                <li><strong>Contract Agreement:</strong> Finalize project terms, payment schedule, and milestones</li>
                <li><strong>Progress Updates:</strong> Keep the client informed throughout the construction process</li>
            </ol>

            <p><strong>Professional Guidelines:</strong></p>
            <ul>
                <li>Maintain open and transparent communication with the client</li>
                <li>Provide realistic timelines and cost estimates</li>
                <li>Use quality materials and workmanship</li>
                <li>Comply with all local building codes and regulations</li>
                <li>Document progress with photos and reports</li>
                <li>Address client concerns promptly and professionally</li>
            </ul>

            <p><strong>Project Documentation:</strong></p>
            <ul>
                <li>Complete design files are available to the client in their account</li>
                <li>Request access to design files if needed for accurate quoting</li>
                <li>Ensure all contracts are documented in writing</li>
                <li>Keep records of all communications and changes</li>
            </ul>

            <div class=""highlight"">
                <strong>💼 Platform Commission:</strong>
                <p>A 2% platform fee (KES {(estimatedCost * 0.02m):N2}) is included in this project. This fee supports platform maintenance, quality assurance, and dispute resolution services.</p>
            </div>

            <p>We're excited to have you work on this project! If you have any questions or need support, please contact our contractor support team.</p>

            <a href=""https://planmorph.com/contractor/projects"" class=""button"">View Project Dashboard</a>
        </div>
        <div class=""footer"">
            <p>Contractor Support: contractors@planmorph.com | Phone: [To be added]</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    // Support Ticket Email Methods
    public async Task SendTicketCreatedEmailAsync(string toEmail, string subject, string ticketId, string priority, string category)
    {
        var emailSubject = $"Support Ticket Created - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .ticket-info {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎫 Support Ticket Created</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>Your support ticket has been successfully created. Our support team will review your request and respond as soon as possible.</p>
            
            <div class=""ticket-info"">
                <h3>Ticket Details:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Priority:</strong> {priority}</p>
                <p><strong>Category:</strong> {category}</p>
                <p><strong>Status:</strong> Open</p>
            </div>
            
            <p><strong>What happens next?</strong></p>
            <ul>
                <li>Our support team will review your ticket within 24 hours</li>
                <li>You'll receive email updates when we respond or when the status changes</li>
                <li>You can reply to ticket emails or visit your support dashboard to add more information</li>
            </ul>
            
            <a href=""https://planmorph.com/support/tickets/{ticketId}"" class=""button"">View Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    public async Task SendNewTicketAlertEmailAsync(string ticketId, string subject, string clientId, string priority, string category)
    {
        var adminEmail = "admin@planmorph.com"; // This should come from configuration
        var emailSubject = $"🚨 New Support Ticket: {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .ticket-info {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #ff6b6b; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🚨 New Support Ticket Alert</h1>
        </div>
        <div class=""content"">
            <p>A new support ticket has been created and requires attention.</p>
            
            <div class=""ticket-info"">
                <h3>Ticket Details:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Client ID:</strong> {clientId}</p>
                <p><strong>Priority:</strong> {priority}</p>
                <p><strong>Category:</strong> {category}</p>
            </div>
            
            <p>Please review and assign this ticket promptly based on the priority level.</p>
            
            <a href=""https://planmorph.com/admin/tickets/{ticketId}"" class=""button"">View & Assign Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Admin Dashboard</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(adminEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketStatusChangedEmailAsync(string toEmail, string ticketId, string subject, string previousStatus, string newStatus)
    {
        var emailSubject = $"Ticket Status Updated - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .status-change {{ background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📋 Ticket Status Updated</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>The status of your support ticket has been updated.</p>
            
            <div class=""status-change"">
                <h3>Status Change:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Previous Status:</strong> {previousStatus}</p>
                <p><strong>New Status:</strong> {newStatus}</p>
            </div>
            
            <p>You can view the full ticket details and any new messages by clicking the button below.</p>
            
            <a href=""https://planmorph.com/support/tickets/{ticketId}"" class=""button"">View Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketAssignedEmailAsync(string toEmail, string ticketId, string subject, string adminName)
    {
        var emailSubject = $"Ticket Assigned - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #17a2b8 0%, #138496 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .assignment-info {{ background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #17a2b8; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>👤 Ticket Assigned</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>Great news! Your support ticket has been assigned to a team member who will help resolve your issue.</p>
            
            <div class=""assignment-info"">
                <h3>Assignment Details:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Assigned to:</strong> {adminName}</p>
            </div>
            
            <p>You can expect to hear from them soon. They'll work with you to resolve your request as quickly as possible.</p>
            
            <a href=""https://planmorph.com/support/tickets/{ticketId}"" class=""button"">View Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketReplyEmailAsync(string toEmail, string ticketId, string subject, string messageContent, string replyFromName)
    {
        var emailSubject = $"New Reply - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #6f42c1 0%, #6610f2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .message {{ background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #6f42c1; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6f42c1; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>💬 New Reply to Your Ticket</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>You have received a new reply to your support ticket from {replyFromName}.</p>
            
            <p><strong>Ticket ID:</strong> #{ticketId}</p>
            <p><strong>Subject:</strong> {subject}</p>
            
            <div class=""message"">
                <h4>Message from {replyFromName}:</h4>
                <p>{messageContent}</p>
            </div>
            
            <p>You can reply directly to this email or visit your support dashboard to continue the conversation.</p>
            
            <a href=""https://planmorph.com/support/tickets/{ticketId}"" class=""button"">Reply to Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketUpdatedEmailAsync(string ticketId, string subject, string clientId, string messageContent)
    {
        var adminEmail = "admin@planmorph.com"; // This should come from configuration
        var emailSubject = $"Ticket Updated by Client - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #fd7e14 0%, #e55e14 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .message {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #fd7e14; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #fd7e14; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>📝 Ticket Updated by Client</h1>
        </div>
        <div class=""content"">
            <p>A client has updated their support ticket and requires attention.</p>
            
            <p><strong>Ticket ID:</strong> #{ticketId}</p>
            <p><strong>Subject:</strong> {subject}</p>
            <p><strong>Client ID:</strong> {clientId}</p>
            
            <div class=""message"">
                <h4>Client Message:</h4>
                <p>{messageContent}</p>
            </div>
            
            <p>Please review and respond to the client's message promptly.</p>
            
            <a href=""https://planmorph.com/admin/tickets/{ticketId}"" class=""button"">View & Respond</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Admin Dashboard</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(adminEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketClosedEmailAsync(string toEmail, string ticketId, string subject)
    {
        var emailSubject = $"Ticket Closed - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .closure-info {{ background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
        .feedback {{ background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ Ticket Closed</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>Your support ticket has been marked as resolved and closed.</p>
            
            <div class=""closure-info"">
                <h3>Closure Details:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Status:</strong> Closed</p>
            </div>
            
            <div class=""feedback"">
                <h4>We'd love your feedback!</h4>
                <p>How was your support experience? Your feedback helps us improve our service.</p>
                <p>If you need further assistance with this issue, simply reply to this email and the ticket will be reopened.</p>
            </div>
            
            <a href=""https://planmorph.com/support/feedback/{ticketId}"" class=""button"">Leave Feedback</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    public async Task SendTicketReopenedEmailAsync(string toEmail, string ticketId, string subject)
    {
        var emailSubject = $"Ticket Reopened - {subject}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ffc107 0%, #e0a800 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ padding: 20px; }}
        .reopen-info {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #ffc107; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔄 Ticket Reopened</h1>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            
            <p>Your support ticket has been reopened after you added a new message.</p>
            
            <div class=""reopen-info"">
                <h3>Reopened Ticket:</h3>
                <p><strong>Ticket ID:</strong> #{ticketId}</p>
                <p><strong>Subject:</strong> {subject}</p>
                <p><strong>Status:</strong> Open</p>
            </div>
            
            <p>Our support team will review your new message and respond as soon as possible.</p>
            
            <a href=""https://planmorph.com/support/tickets/{ticketId}"" class=""button"">View Ticket</a>
        </div>
        <div class=""footer"">
            <p>PlanMorph Support Team | support@planmorph.com</p>
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, emailSubject, htmlBody);
    }

    // === STUDENT MENTORSHIP EMAILS ===

    public async Task SendStudentApplicationReceivedEmailAsync(string toEmail, string studentName)
    {
        var subject = "Application Received - PlanMorph Mentorship Program";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6366f1; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Received</h1>
        </div>
        <div class=""content"">
            <p>Hi {studentName},</p>
            <p>Thank you for applying to the <strong>PlanMorph Learn While You Earn</strong> mentorship program.</p>
            <p>Your application has been received and is being reviewed by our team. We'll notify you once a decision has been made.</p>
            <p><strong>What to expect:</strong></p>
            <ul>
                <li>Our team will review your credentials and qualifications</li>
                <li>You'll receive an email with the outcome</li>
                <li>If approved, you'll gain access to the student portal</li>
            </ul>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendAdminNewStudentApplicationEmailAsync(string studentName, string studentType, string universityName)
    {
        var subject = $"New Student Application - {studentName}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc2626; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .info-box {{ background-color: #fff; border: 1px solid #e5e7eb; border-radius: 6px; padding: 15px; margin: 15px 0; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6366f1; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>New Student Application</h1>
        </div>
        <div class=""content"">
            <p>A new student has applied to the mentorship program.</p>
            <div class=""info-box"">
                <p><strong>Name:</strong> {studentName}</p>
                <p><strong>Type:</strong> {studentType}</p>
                <p><strong>University:</strong> {universityName}</p>
            </div>
            <a href=""https://planmorph.com/admin"" class=""button"">Review Application</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync("admin@planmorph.com", subject, htmlBody);
    }

    public async Task SendStudentApplicationApprovedEmailAsync(string toEmail, string studentName)
    {
        var subject = "Welcome to PlanMorph Mentorship Program!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6366f1; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Approved!</h1>
        </div>
        <div class=""content"">
            <p>Hi {studentName},</p>
            <p>Congratulations! Your application to the <strong>PlanMorph Learn While You Earn</strong> mentorship program has been approved.</p>
            <p>You can now log in to your student portal and start working on mentored projects.</p>
            <p><strong>Next steps:</strong></p>
            <ul>
                <li>Log in to your student dashboard</li>
                <li>Complete your profile</li>
                <li>Wait for a mentor to assign you to a project</li>
                <li>Start earning while you learn!</li>
            </ul>
            <a href=""https://planmorph.com/student/login"" class=""button"">Go to Student Portal</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendStudentApplicationRejectedEmailAsync(string toEmail, string studentName, string? reason = null)
    {
        var reasonText = string.IsNullOrEmpty(reason)
            ? "your application did not meet our current requirements"
            : reason;

        var subject = "PlanMorph Mentorship Application Update";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6b7280; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Application Update</h1>
        </div>
        <div class=""content"">
            <p>Hi {studentName},</p>
            <p>Thank you for your interest in the PlanMorph mentorship program.</p>
            <p>After careful review, we regret to inform you that {reasonText}.</p>
            <p>You are welcome to reapply in the future once you have additional experience or qualifications.</p>
            <p>If you have questions about this decision, please contact our support team.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendStudentCredentialsEmailAsync(string toEmail, string studentName, string temporaryPassword)
    {
        var subject = "Your PlanMorph Student Account Credentials";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .credentials-box {{ background-color: #fff; border: 2px solid #6366f1; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .credentials-box p {{ margin: 8px 0; }}
        .password {{ font-family: monospace; font-size: 18px; background-color: #f3f4f6; padding: 8px 12px; border-radius: 4px; display: inline-block; letter-spacing: 1px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6366f1; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Welcome to PlanMorph!</h1>
        </div>
        <div class=""content"">
            <p>Hi {studentName},</p>

            <p>Congratulations! Your application to the <strong>PlanMorph Learn While You Earn</strong> mentorship program has been approved.</p>

            <p>Your student account has been created. Use the credentials below to log in:</p>

            <div class=""credentials-box"">
                <h3 style=""margin-top: 0; color: #6366f1;"">Your Login Credentials</h3>
                <p><strong>Email:</strong> {toEmail}</p>
                <p><strong>Temporary Password:</strong> <span class=""password"">{temporaryPassword}</span></p>
            </div>

            <div class=""warning"">
                <strong>Important:</strong> Please change your password after your first login for security purposes. Keep these credentials safe and do not share them with anyone.
            </div>

            <p><strong>Next steps:</strong></p>
            <ul>
                <li>Log in to your student dashboard using the credentials above</li>
                <li>Complete your profile</li>
                <li>Wait for a mentor to assign you to a project</li>
                <li>Start earning while you learn!</li>
            </ul>

            <a href=""https://planmorph.com/student/login"" class=""button"">Log In to Student Portal</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendMentorStudentInvitationEmailAsync(string toEmail, string studentName, string mentorName)
    {
        var subject = $"You've Been Invited to PlanMorph by {mentorName}!";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6366f1; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #6366f1; color: white; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>You're Invited!</h1>
        </div>
        <div class=""content"">
            <p>Hi {studentName},</p>
            <p><strong>{mentorName}</strong>, a licensed professional on PlanMorph, has invited you to join the <strong>Learn While You Earn</strong> mentorship program.</p>
            <p>As a mentored design assistant, you'll work on real client projects under professional supervision while earning per-project compensation.</p>
            <p><strong>Benefits:</strong></p>
            <ul>
                <li>Work on real architectural and engineering projects</li>
                <li>Learn from licensed professionals</li>
                <li>Earn money while building your portfolio</li>
                <li>Get mentored feedback on your work</li>
            </ul>
            <a href=""https://planmorph.com/student/register"" class=""button"">Accept Invitation & Register</a>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    // === MENTORSHIP PAYMENT EMAILS ===

    public async Task SendMentorshipPaymentConfirmationEmailAsync(string toEmail, string clientName, string projectTitle, decimal amount, string projectNumber)
    {
        var subject = $"Payment Confirmed - {projectNumber}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .payment-box {{ background-color: #fff; border: 1px solid #e5e7eb; border-radius: 6px; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Payment Confirmed</h1>
        </div>
        <div class=""content"">
            <p>Hi {clientName},</p>
            <p>Your payment for the mentorship project has been confirmed and funds are now held securely in escrow.</p>
            <div class=""payment-box"">
                <p><strong>Project:</strong> {projectTitle}</p>
                <p><strong>Project Number:</strong> {projectNumber}</p>
                <p><strong>Amount:</strong> KES {amount:N2}</p>
                <p><strong>Status:</strong> Escrowed (held securely)</p>
            </div>
            <p>Your funds will be held in escrow until you accept the final deliverable. You are fully protected throughout the process.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendMentorshipPaymentReleasedEmailAsync(string toEmail, string recipientName, string projectTitle, decimal amount, string role)
    {
        var subject = $"Payment Released - {projectTitle}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .payment-box {{ background-color: #ecfdf5; border: 1px solid #a7f3d0; border-radius: 6px; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Payment Released!</h1>
        </div>
        <div class=""content"">
            <p>Hi {recipientName},</p>
            <p>Great news! Your {role.ToLower()} payment for the project <strong>{projectTitle}</strong> has been released.</p>
            <div class=""payment-box"">
                <p><strong>Amount:</strong> KES {amount:N2}</p>
                <p><strong>Role:</strong> {role}</p>
                <p><strong>Status:</strong> Released</p>
            </div>
            <p>Thank you for your contribution to this project. The payment will be processed to your account.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task SendMentorshipRefundEmailAsync(string toEmail, string clientName, string projectTitle, decimal amount, string refundType, string? reason)
    {
        var reasonText = string.IsNullOrEmpty(reason) ? "" : $"<p><strong>Reason:</strong> {reason}</p>";
        var subject = $"Refund Processed - {projectTitle}";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 30px; border-radius: 0 0 8px 8px; }}
        .refund-box {{ background-color: #eff6ff; border: 1px solid #bfdbfe; border-radius: 6px; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Refund Processed</h1>
        </div>
        <div class=""content"">
            <p>Hi {clientName},</p>
            <p>A refund has been processed for your mentorship project.</p>
            <div class=""refund-box"">
                <p><strong>Project:</strong> {projectTitle}</p>
                <p><strong>Original Amount:</strong> KES {amount:N2}</p>
                <p><strong>Refund Type:</strong> {refundType}</p>
                {reasonText}
            </div>
            <p>The refund will be returned to your original payment method. Processing may take 3-5 business days.</p>
            <p>If you have questions about this refund, please contact our support team.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 PlanMorph. Building Dreams Worldwide.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, htmlBody);
    }
}
