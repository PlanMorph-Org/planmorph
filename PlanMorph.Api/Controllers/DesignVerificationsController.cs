using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;
using System.Security.Claims;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/design-verifications")]
public class DesignVerificationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DesignVerificationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [Authorize(Roles = "Architect,Engineer,Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingVerifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var allVerifications = await _unitOfWork.DesignVerifications.FindAsync(v => v.Status == VerificationStatus.Pending);

        // Filter based on role
        var filteredVerifications = allVerifications.Where(v =>
        {
            if (userRole == "Admin") return true;
            if (userRole == "Architect")
            {
                return v.VerificationType == VerificationType.Architectural
                    || v.VerificationType == VerificationType.BOQ
                    || v.VerificationType == VerificationType.BOQArchitect;
            }
            if (userRole == "Engineer")
            {
                return v.VerificationType == VerificationType.Structural
                    || v.VerificationType == VerificationType.BOQ
                    || v.VerificationType == VerificationType.BOQEngineer;
            }
            return false;
        });

        var result = new List<VerificationDto>();
        foreach (var verification in filteredVerifications)
        {
            var design = await _unitOfWork.Designs.GetByIdAsync(verification.DesignId);
            if (design == null) continue;

            var architect = await _unitOfWork.Users.GetByIdAsync(design.ArchitectId);

            result.Add(new VerificationDto
            {
                Id = verification.Id,
                DesignId = design.Id,
                DesignTitle = design.Title,
                ArchitectName = architect != null ? $"{architect.FirstName} {architect.LastName}" : "Unknown",
                VerificationType = verification.VerificationType.ToString(),
                Status = verification.Status.ToString(),
                Comments = verification.Comments,
                CreatedAt = verification.CreatedAt
            });
        }

        return Ok(result);
    }

    [Authorize(Roles = "Architect,Engineer,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVerification(Guid id)
    {
        var verification = await _unitOfWork.DesignVerifications.GetByIdAsync(id);
        if (verification == null)
            return NotFound(new { message = "Verification not found" });

        var design = await _unitOfWork.Designs.GetByIdAsync(verification.DesignId);
        if (design == null)
            return NotFound(new { message = "Design not found" });

        var architect = await _unitOfWork.Users.GetByIdAsync(design.ArchitectId);

        var dto = new VerificationDto
        {
            Id = verification.Id,
            DesignId = design.Id,
            DesignTitle = design.Title,
            ArchitectName = architect != null ? $"{architect.FirstName} {architect.LastName}" : "Unknown",
            VerificationType = verification.VerificationType.ToString(),
            Status = verification.Status.ToString(),
            Comments = verification.Comments,
            CreatedAt = verification.CreatedAt
        };

        return Ok(dto);
    }

    [Authorize(Roles = "Architect,Engineer,Admin")]
    [HttpGet("{id}/files")]
    public async Task<IActionResult> GetVerificationFiles(Guid id)
    {
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var verification = await _unitOfWork.DesignVerifications.GetByIdAsync(id);
        if (verification == null)
            return NotFound(new { message = "Verification not found" });

        bool canAccess = verification.VerificationType switch
        {
            VerificationType.Architectural => userRole == "Architect",
            VerificationType.Structural => userRole == "Engineer",
            VerificationType.BOQArchitect => userRole == "Architect",
            VerificationType.BOQEngineer => userRole == "Engineer",
            VerificationType.BOQ => userRole == "Architect" || userRole == "Engineer",
            _ => false
        };

        if (!canAccess)
            return Forbid();

        IEnumerable<FileCategory> allowedCategories = verification.VerificationType switch
        {
            VerificationType.Architectural => new[]
            {
                FileCategory.ArchitecturalDrawing,
                FileCategory.PreviewImage,
                FileCategory.FullRenderImage
            },
            VerificationType.Structural => new[] { FileCategory.StructuralDrawing },
            VerificationType.BOQArchitect => new[] { FileCategory.BOQ },
            VerificationType.BOQEngineer => new[] { FileCategory.BOQ },
            VerificationType.BOQ => new[] { FileCategory.BOQ },
            _ => Array.Empty<FileCategory>()
        };

        var files = await _unitOfWork.DesignFiles.FindAsync(f =>
            f.DesignId == verification.DesignId && allowedCategories.Contains(f.Category));

        var result = files.Select(f => new VerificationFileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            Category = f.Category.ToString(),
            StorageUrl = f.StorageUrl,
            FileSizeBytes = f.FileSizeBytes
        }).ToList();

        return Ok(result);
    }

    [Authorize(Roles = "Architect,Engineer,Admin")]
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyDesign(Guid id, [FromBody] VerificationActionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var verification = await _unitOfWork.DesignVerifications.GetByIdAsync(id);
        if (verification == null)
            return NotFound(new { message = "Verification not found" });

        // Check if user has permission to verify this type
        bool canVerify = verification.VerificationType switch
        {
            VerificationType.Architectural => userRole == "Architect",
            VerificationType.Structural => userRole == "Engineer",
            VerificationType.BOQArchitect => userRole == "Architect",
            VerificationType.BOQEngineer => userRole == "Engineer",
            VerificationType.BOQ => userRole == "Architect" || userRole == "Engineer",
            _ => false
        };

        if (!canVerify)
            return Forbid();

        verification.Status = VerificationStatus.Verified;
        verification.VerifierUserId = userId;
        verification.Comments = dto.Comments;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.DesignVerifications.UpdateAsync(verification);

        // Check if all verifications for this design are complete
        var allVerifications = await _unitOfWork.DesignVerifications.FindAsync(v => v.DesignId == verification.DesignId);
        bool allVerified = allVerifications.All(v => v.Status == VerificationStatus.Verified);

        if (allVerified)
        {
            var design = await _unitOfWork.Designs.GetByIdAsync(verification.DesignId);
            if (design != null && design.Status == DesignStatus.PendingVerification)
            {
                design.Status = DesignStatus.PendingApproval;
                design.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Designs.UpdateAsync(design);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Design verified successfully" });
    }

    [Authorize(Roles = "Architect,Engineer,Admin")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectVerification(Guid id, [FromBody] VerificationActionDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var verification = await _unitOfWork.DesignVerifications.GetByIdAsync(id);
        if (verification == null)
            return NotFound(new { message = "Verification not found" });

        // Check if user has permission to verify this type
        bool canVerify = verification.VerificationType switch
        {
            VerificationType.Architectural => userRole == "Architect",
            VerificationType.Structural => userRole == "Engineer",
            VerificationType.BOQArchitect => userRole == "Architect",
            VerificationType.BOQEngineer => userRole == "Engineer",
            VerificationType.BOQ => userRole == "Architect" || userRole == "Engineer",
            _ => false
        };

        if (!canVerify)
            return Forbid();

        verification.Status = VerificationStatus.Rejected;
        verification.VerifierUserId = userId;
        verification.Comments = dto.Comments;
        verification.VerifiedAt = DateTime.UtcNow;
        verification.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.DesignVerifications.UpdateAsync(verification);

        // Reject the design as well
        var design = await _unitOfWork.Designs.GetByIdAsync(verification.DesignId);
        if (design != null)
        {
            design.Status = DesignStatus.Rejected;
            design.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Designs.UpdateAsync(design);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Verification rejected" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("design/{designId}")]
    public async Task<IActionResult> GetDesignVerifications(Guid designId)
    {
        var verifications = await _unitOfWork.DesignVerifications.FindAsync(v => v.DesignId == designId);

        var result = verifications.Select(v => new VerificationDto
        {
            Id = v.Id,
            DesignId = v.DesignId,
            DesignTitle = "",
            ArchitectName = "",
            VerificationType = v.VerificationType.ToString(),
            Status = v.Status.ToString(),
            Comments = v.Comments,
            CreatedAt = v.CreatedAt,
            VerifiedAt = v.VerifiedAt
        }).ToList();

        return Ok(result);
    }
}

public class VerificationDto
{
    public Guid Id { get; set; }
    public Guid DesignId { get; set; }
    public string DesignTitle { get; set; } = "";
    public string ArchitectName { get; set; } = "";
    public string VerificationType { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class VerificationFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public string Category { get; set; } = "";
    public string StorageUrl { get; set; } = "";
    public long FileSizeBytes { get; set; }
}

public class VerificationActionDto
{
    public string? Comments { get; set; }
}
