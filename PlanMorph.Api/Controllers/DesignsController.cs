using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs.Design;
using PlanMorph.Application.Services;
using System.Security.Claims;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesignsController : ControllerBase
{
    private readonly IDesignService _designService;
    private readonly IFileUploadService _fileUploadService;

    public DesignsController(IDesignService designService, IFileUploadService fileUploadService)
    {
        _designService = designService;
        _fileUploadService = fileUploadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDesigns()
    {
        var designs = await _designService.GetApprovedDesignsAsync();
        return Ok(designs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDesign(Guid id)
    {
        var design = await _designService.GetDesignByIdAsync(id);
        
        if (design == null)
            return NotFound();

        return Ok(design);
    }

    [HttpPost("filter")]
    public async Task<IActionResult> FilterDesigns([FromBody] DesignFilterDto filter)
    {
        var designs = await _designService.FilterDesignsAsync(filter);
        return Ok(designs);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost]
    public async Task<IActionResult> CreateDesign([FromBody] CreateDesignDto createDesignDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return BadRequest(new { message = "User ID not found in token" });
        }

        if (!Guid.TryParse(userIdClaim, out var architectId))
        {
            return BadRequest(new { message = "Invalid user ID format" });
        }

        var design = await _designService.CreateDesignAsync(createDesignDto, architectId);

        if (design == null)
            return BadRequest(new { message = "Failed to create design" });

        return CreatedAtAction(nameof(GetDesign), new { id = design.Id }, design);
    }

    [Authorize(Roles = "Architect")]
    [HttpGet("my-designs")]
    public async Task<IActionResult> GetMyDesigns()
    {
        var architectId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var designs = await _designService.GetDesignsByArchitectAsync(architectId);
        return Ok(designs);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveDesign(Guid id)
    {
        var result = await _designService.ApproveDesignAsync(id);
        
        if (!result)
            return NotFound();

        return Ok(new { message = "Design approved successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectDesign(Guid id)
    {
        var result = await _designService.RejectDesignAsync(id);
        
        if (!result)
            return NotFound();

        return Ok(new { message = "Design rejected successfully" });
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{id}/files")]
    public async Task<IActionResult> UploadFiles(Guid id, [FromForm] IFormFileCollection files, [FromQuery] string category)
    {
        if (string.IsNullOrEmpty(category))
            return BadRequest(new { message = "Category parameter is required" });

        if (!Enum.TryParse<Core.Entities.FileCategory>(category, true, out var fileCategory))
            return BadRequest(new { message = $"Invalid file category: {category}" });

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files provided" });

        var uploadedFiles = await _fileUploadService.UploadMultipleFilesAsync(
            id,
            files.ToList(),
            fileCategory
        );

        var result = uploadedFiles.Select(f => new DesignFileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            FileType = f.FileType.ToString(),
            Category = f.Category.ToString(),
            ThumbnailUrl = f.ThumbnailUrl,
            FileSizeBytes = f.FileSizeBytes
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpGet("files/{fileId}/download")]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            var downloadUrl = await _fileUploadService.GetSecureDownloadUrlAsync(fileId, userId);
            return Ok(new { url = downloadUrl });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("files/{fileId}/admin-download")]
    public async Task<IActionResult> GetAdminDownloadUrl(Guid fileId)
    {
        try
        {
            var downloadUrl = await _fileUploadService.GetAdminDownloadUrlAsync(fileId);
            return Ok(new { url = downloadUrl });
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}
