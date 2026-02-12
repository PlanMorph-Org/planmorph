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

        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@planmorph.com";
        _fromName = _configuration["EmailSettings:FromName"] ?? "PlanMorph";
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
}
