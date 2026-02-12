using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs.Modification;
using PlanMorph.Application.Services;
using System.Security.Claims;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModificationRequestsController : ControllerBase
{
    private readonly IModificationRequestService _modificationService;

    public ModificationRequestsController(IModificationRequestService modificationService)
    {
        _modificationService = modificationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateModificationRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var request = await _modificationService.CreateRequestAsync(dto, userId);

        if (request == null)
            return BadRequest(new { message = "Failed to create modification request" });

        return CreatedAtAction(nameof(GetRequest), new { id = request.Id }, request);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequest(Guid id)
    {
        var request = await _modificationService.GetRequestByIdAsync(id);
        
        if (request == null)
            return NotFound();

        return Ok(request);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetRequestsByOrder(Guid orderId)
    {
        var requests = await _modificationService.GetRequestsByOrderAsync(orderId);
        return Ok(requests);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{id}/quote")]
    public async Task<IActionResult> SubmitQuote(Guid id, [FromBody] SubmitQuoteDto dto)
    {
        var result = await _modificationService.SubmitQuoteAsync(id, dto.QuotedPrice);

        if (!result)
            return BadRequest(new { message = "Failed to submit quote" });

        return Ok(new { message = "Quote submitted successfully" });
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptQuote(Guid id)
    {
        var result = await _modificationService.AcceptQuoteAsync(id);

        if (!result)
            return BadRequest(new { message = "Failed to accept quote" });

        return Ok(new { message = "Quote accepted successfully" });
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id)
    {
        var result = await _modificationService.RejectRequestAsync(id);

        if (!result)
            return BadRequest(new { message = "Failed to reject request" });

        return Ok(new { message = "Request rejected successfully" });
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        var result = await _modificationService.MarkAsCompletedAsync(id);

        if (!result)
            return BadRequest(new { message = "Failed to mark as completed" });

        return Ok(new { message = "Request marked as completed" });
    }
}

public class SubmitQuoteDto
{
    public decimal QuotedPrice { get; set; }
}