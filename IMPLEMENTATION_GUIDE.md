# PlanMorph - API Implementation Guide

## Recently Implemented Critical Features

This document outlines the critical features that were recently implemented to complete the PlanMorph platform.

## 1. Email Notification System ✅

### What Was Implemented

A comprehensive email notification service that sends professional HTML emails for all critical user interactions.

### Files Created/Modified

- `PlanMorph.Application/Services/IEmailService.cs` - Email service interface
- `PlanMorph.Infrastructure/Services/EmailService.cs` - SMTP email implementation
- `PlanMorph.Application/Services/DesignService.cs` - Added email notifications for design approval/rejection
- `PlanMorph.Application/Services/AuthService.cs` - Added welcome emails for new clients
- `PlanMorph.Application/Services/OrderService.cs` - Added order confirmation emails

### Email Templates Implemented

1. **Design Approved Email** - Sent to architects when their design is approved
2. **Design Rejected Email** - Sent to architects when their design is rejected (with optional reason)
3. **Architect Approved Email** - Sent when an architect application is approved
4. **Architect Rejected Email** - Sent when an architect application is rejected (with optional reason)
5. **Order Confirmation Email** - Sent to clients when payment is confirmed
6. **Welcome Email** - Sent to new clients when they register
7. **Password Reset Email** - Sent when users request password reset

### Configuration Required

Add the following to `appsettings.json`:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",
  "FromEmail": "noreply@planmorph.software",
  "FromName": "PlanMorph"
}
```

**Important Notes:**
- For Gmail, you need to use an App Password, not your regular password
- Go to Google Account → Security → 2-Step Verification → App Passwords
- If using production email service, use services like SendGrid, AWS SES, or Mailgun for better deliverability

---

## 2. User Management API Endpoints ✅

### What Was Implemented

Complete REST API for user management including architect application approval.

### New Controller: `UsersController.cs`

**Endpoints:**

- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/pending-architects` - Get pending architect applications (Admin only)
- `PUT /api/users/{id}/approve-architect` - Approve architect application (Admin only)
- `DELETE /api/users/{id}/reject-architect` - Reject architect application (Admin only)
- `PUT /api/users/{id}/activate` - Activate user account (Admin only)
- `PUT /api/users/{id}/deactivate` - Deactivate user account (Admin only)
- `POST /api/users/forgot-password` - Request password reset (Public)
- `POST /api/users/reset-password` - Reset password with token (Public)

### Workflow

1. **Architect Registration:** User registers with role "Architect" → Account created with `IsActive = false`
2. **Admin Review:** Admin sees pending applications in `/api/users/pending-architects`
3. **Approval:** Admin calls `/api/users/{id}/approve-architect` → User activated + Email sent
4. **Login:** Architect can now login and start uploading designs

---

## 3. Password Reset Functionality ✅

### What Was Implemented

Complete password reset workflow using ASP.NET Identity's built-in token system.

### Workflow

1. User requests password reset at `/api/users/forgot-password` with email
2. System generates secure reset token (expires in 1 hour)
3. Email sent with reset link containing token
4. User clicks link, enters new password
5. Frontend calls `/api/users/reset-password` with email, token, and new password
6. Password updated successfully

### Frontend Implementation Needed

Create a password reset page at `/reset-password` that:
- Reads token from query parameter
- Shows form for new password
- Calls the reset password endpoint

---

## 4. Global Error Handling Middleware ✅

### What Was Implemented

Professional error handling that catches all unhandled exceptions and returns consistent JSON responses.

### Files Created

- `PlanMorph.Api/Middleware/GlobalExceptionHandlerMiddleware.cs`

### Features

- Catches all unhandled exceptions
- Returns consistent JSON error responses
- Different status codes for different exception types:
  - `UnauthorizedAccessException` → 401
  - `FileNotFoundException` → 404
  - `ArgumentException` → 400
  - `InvalidOperationException` → 400
  - Everything else → 500
- In development mode: includes stack trace
- In production mode: generic error messages (security)
- All errors are logged

### Error Response Format

```json
{
  "statusCode": 500,
  "message": "Error description",
  "details": "Stack trace (dev mode only)"
}
```

---

## 5. Architect Application Workflow ✅

### What Was Already Working

- Architect registration sets `IsActive = false`
- Inactive architects cannot login
- Admin panel shows pending architects
- Admin can approve/reject from UI

### What Was Added

- API endpoints for architect approval (enables mobile app support)
- Email notifications when approved/rejected
- Optional rejection reason
- Welcome email with getting started guide on approval

---

## 6. Design Approval Workflow ✅

### What Was Already Working

- Architects upload designs with status "PendingApproval"
- Admin panel shows pending designs
- Admin can approve/reject from UI

### What Was Added

- Email notifications to architects when design is approved/rejected
- Optional rejection reason in emails
- Guidance on next steps in emails

---

## Configuration Checklist

### 1. Database Setup

Already configured with:
- PostgreSQL (Neon serverless)
- Connection string in `appsettings.json`
- Auto-migrations on startup

### 2. Email Configuration

**Required:** Update `appsettings.json` with SMTP credentials

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUsername": "YOUR_EMAIL",
  "SmtpPassword": "YOUR_APP_PASSWORD",
  "FromEmail": "noreply@planmorph.software",
  "FromName": "PlanMorph"
}
```

### 3. File Storage Configuration

Already configured with:
- DigitalOcean Spaces
- Credentials in `appsettings.json`

### 4. JWT Configuration

Already configured with:
- Secret key in `appsettings.json`
- 60-minute token expiry

---

## API Testing Guide

### Testing Architect Approval Flow

1. **Register as Architect:**
```bash
POST /api/auth/register
{
  "email": "architect@example.com",
  "password": "Test@123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+254712345678",
  "role": "Architect"
}
```
Response: `"role": "PendingApproval"` (no token)

2. **Admin Login:**
```bash
POST /api/auth/login
{
  "email": "<admin-email-from-env>",
  "password": "<admin-password-from-env>"
}
```
Save the token from response.

3. **Get Pending Architects:**
```bash
GET /api/users/pending-architects
Authorization: Bearer <admin-token>
```

4. **Approve Architect:**
```bash
PUT /api/users/{architect-id}/approve-architect
Authorization: Bearer <admin-token>
```
Architect receives approval email.

5. **Architect Can Now Login:**
```bash
POST /api/auth/login
{
  "email": "architect@example.com",
  "password": "Test@123"
}
```
Now succeeds and returns token!

### Testing Design Approval Flow

1. **Architect Uploads Design:**
```bash
POST /api/designs
Authorization: Bearer <architect-token>
{
  "title": "Modern 3BR Bungalow",
  "description": "Beautiful modern design",
  "price": 50000,
  "bedrooms": 3,
  "bathrooms": 2,
  "squareFootage": 1500,
  "stories": 1,
  "category": "Bungalow",
  "estimatedConstructionCost": 5000000
}
```

2. **Admin Approves Design:**
```bash
PUT /api/designs/{design-id}/approve
Authorization: Bearer <admin-token>
```
Architect receives email notification.

3. **Verify Design is Public:**
```bash
GET /api/designs
```
Design now appears in public listing.

### Testing Password Reset

1. **Request Reset:**
```bash
POST /api/users/forgot-password
{
  "email": "user@example.com"
}
```
User receives email with token.

2. **Reset Password:**
```bash
POST /api/users/reset-password
{
  "email": "user@example.com",
  "token": "<token-from-email>",
  "newPassword": "NewPassword@123"
}
```

---

## Integration with Existing Systems

### Admin Panel (Blazor)

The existing admin panel already has UI for:
- ✅ Design approval (c:\Users\larry\projects\planmorph\PlanMorph.Admin\Pages\Designs\Index.razor)
- ✅ Architect approval (c:\Users\larry\projects\planmorph\PlanMorph.Admin\Pages\Users\Index.razor)

**What Changed:**
- Admin actions now trigger email notifications automatically
- No changes needed to admin panel UI

### Client Frontend (Next.js)

Update required:
1. Add password reset flow (`/reset-password` page)
2. Better error messages for pending architect approval
3. Display success messages after design upload

### Mobile App (Future)

All user management features are now available via API endpoints, making mobile app integration straightforward.

---

## Security Considerations

### Implemented

- ✅ JWT Bearer authentication
- ✅ Role-based authorization
- ✅ Password complexity requirements
- ✅ Global error handling (no sensitive data leaked)
- ✅ Secure password reset tokens (1-hour expiry)

### Still Needed (Future)

- [ ] Refresh tokens
- [ ] Rate limiting
- [ ] Account lockout after failed logins
- [ ] Email verification on registration
- [ ] 2FA for architects and admin
- [ ] HTTPS enforcement in production
- [ ] CORS configuration for production

---

## Email Service Alternatives

If SMTP is not reliable, consider these alternatives:

### 1. SendGrid (Recommended)
- Free tier: 100 emails/day
- Easy setup, high deliverability
- Install: `SendGrid` NuGet package

### 2. AWS SES
- Pay per email (very cheap)
- Requires AWS account
- Best for production

### 3. Mailgun
- Free tier: 5,000 emails/month
- Good API

### 4. Azure Communication Services
- Good for Azure-deployed apps

---

## Testing Without Email Configuration

If you want to test without setting up SMTP:

The email service includes a safety check:
```csharp
if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
{
    _logger.LogWarning("Email service not configured. Email not sent...");
    return;
}
```

**Result:** Application works normally, but emails are logged instead of sent. Check the application logs to see what emails would have been sent.

---

## Summary of Changes

### New Files Created
1. `IEmailService.cs` - Email service interface
2. `EmailService.cs` - SMTP implementation with HTML templates
3. `UsersController.cs` - User management API endpoints
4. `GlobalExceptionHandlerMiddleware.cs` - Error handling

### Files Modified
1. `DesignService.cs` - Added email notifications
2. `AuthService.cs` - Added welcome emails
3. `OrderService.cs` - Added order confirmation emails
4. `Program.cs` - Registered new services and middleware
5. `appsettings.json` - Added email configuration

### Dependencies Added
- No new NuGet packages required (uses built-in .NET SMTP)

---

## Next Steps

1. **Configure Email:** Add SMTP credentials to `appsettings.json`
2. **Test Workflows:** Follow the testing guide above
3. **Update Client:** Add password reset page
4. **Production Setup:** Consider SendGrid/AWS SES for emails
5. **Security Hardening:** Implement refresh tokens, rate limiting, email verification

---

## Support

For questions or issues:
- Check application logs for detailed error messages
- Verify email configuration with test emails
- Use Swagger UI for API testing: `/swagger`
- Check pending architects/designs in admin panel

---

**Implementation Date:** February 2026
**Status:** ✅ Complete and Ready for Testing
