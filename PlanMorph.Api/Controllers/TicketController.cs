using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Services;
using System.Security.Claims;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private Guid GetCurrentClientId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private Guid? GetAdminId()
    {
        if (!IsAdmin()) return null;
        var clientId = GetCurrentClientId();
        return clientId != Guid.Empty ? clientId : null;
    }

    // Client endpoints
    [HttpPost]
    public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto createTicketDto)
    {
        var clientId = GetCurrentClientId();
        if (clientId == Guid.Empty)
            return Unauthorized();

        var ticket = await _ticketService.CreateTicketAsync(
            clientId,
            createTicketDto.Subject,
            createTicketDto.Description,
            createTicketDto.Category,
            createTicketDto.Priority,
            createTicketDto.OrderId,
            createTicketDto.DesignId
        );

        var ticketDto = MapToTicketDto(ticket, clientId);
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticketDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
    {
        var clientId = GetCurrentClientId();
        if (clientId == Guid.Empty)
            return Unauthorized();

        IEnumerable<Ticket> tickets;
        
        if (IsAdmin())
        {
            tickets = await _ticketService.GetAllTicketsAsync();
        }
        else
        {
            tickets = await _ticketService.GetClientTicketsAsync(clientId);
        }

        var ticketDtos = tickets.Select(t => MapToTicketDto(t, clientId)).ToList();
        return Ok(ticketDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetTicket(Guid id)
    {
        var clientId = GetCurrentClientId();
        if (clientId == Guid.Empty)
            return Unauthorized();

        Ticket? ticket;
        
        if (IsAdmin())
        {
            ticket = await _ticketService.GetTicketWithMessagesAsync(id);
        }
        else
        {
            ticket = await _ticketService.GetTicketByIdAsync(id, clientId);
            if (ticket != null)
            {
                // Mark messages as read when client views ticket
                await _ticketService.MarkMessagesAsReadAsync(id, clientId);
            }
        }

        if (ticket == null)
            return NotFound();

        var ticketDto = MapToTicketDto(ticket, clientId);
        return Ok(ticketDto);
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult> AddMessage(Guid id, AddMessageDto addMessageDto)
    {
        var clientId = GetCurrentClientId();
        if (clientId == Guid.Empty)
            return Unauthorized();

        var isAdmin = IsAdmin();
        
        // Verify access to ticket
        if (!isAdmin)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id, clientId);
            if (ticket == null)
                return NotFound();
        }

        var success = await _ticketService.AddMessageAsync(id, clientId, addMessageDto.Content, isAdmin);
        
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpPut("{id}/close")]
    public async Task<ActionResult> CloseTicket(Guid id)
    {
        var clientId = GetCurrentClientId();
        if (clientId == Guid.Empty)
            return Unauthorized();

        bool success;
        
        if (IsAdmin())
        {
            var adminId = GetAdminId();
            if (!adminId.HasValue) return Unauthorized();
            success = await _ticketService.UpdateTicketStatusAsync(id, TicketStatus.Closed, adminId.Value);
        }
        else
        {
            success = await _ticketService.CloseTicketAsync(id, clientId);
        }

        if (!success)
            return NotFound();

        return Ok();
    }

    // Admin-only endpoints
    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketStatsDto>> GetTicketStats()
    {
        var statusStats = await _ticketService.GetTicketStatusStatsAsync();
        var priorityStats = await _ticketService.GetTicketPriorityStatsAsync();
        var categoryStats = await _ticketService.GetTicketCategoryStatsAsync();

        var totalTickets = statusStats.Values.Sum();
        var openTickets = statusStats.Where(s => s.Key != TicketStatus.Closed).Sum(s => s.Value);
        var closedTickets = statusStats.GetValueOrDefault(TicketStatus.Closed, 0);

        return Ok(new TicketStatsDto
        {
            StatusStats = statusStats,
            PriorityStats = priorityStats,
            CategoryStats = categoryStats,
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            ClosedTickets = closedTickets
        });
    }

    [HttpGet("admin/by-status/{status}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByStatus(TicketStatus status)
    {
        var tickets = await _ticketService.GetTicketsByStatusAsync(status);
        var ticketDtos = tickets.Select(t => MapToTicketDto(t, Guid.Empty)).ToList();
        return Ok(ticketDtos);
    }

    [HttpGet("admin/by-priority/{priority}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByPriority(TicketPriority priority)
    {
        var tickets = await _ticketService.GetTicketsByPriorityAsync(priority);
        var ticketDtos = tickets.Select(t => MapToTicketDto(t, Guid.Empty)).ToList();
        return Ok(ticketDtos);
    }

    [HttpGet("admin/assigned")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAssignedTickets()
    {
        var adminId = GetAdminId();
        if (!adminId.HasValue) return Unauthorized();

        var tickets = await _ticketService.GetAssignedTicketsAsync(adminId.Value);
        var ticketDtos = tickets.Select(t => MapToTicketDto(t, Guid.Empty)).ToList();
        return Ok(ticketDtos);
    }

    [HttpPut("admin/{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateTicketStatus(Guid id, UpdateTicketStatusDto updateStatusDto)
    {
        var adminId = GetAdminId();
        if (!adminId.HasValue) return Unauthorized();

        var success = await _ticketService.UpdateTicketStatusAsync(id, updateStatusDto.Status, adminId.Value);
        
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpPut("admin/{id}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AssignTicket(Guid id, [FromBody] Guid assignToAdminId)
    {
        var success = await _ticketService.AssignTicketAsync(id, assignToAdminId);
        
        if (!success)
            return NotFound();

        return Ok();
    }

    [HttpGet("admin/search")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> SearchTickets([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var tickets = await _ticketService.SearchTicketsAsync(searchTerm);
        var ticketDtos = tickets.Select(t => MapToTicketDto(t, Guid.Empty)).ToList();
        return Ok(ticketDtos);
    }

    private TicketDto MapToTicketDto(Ticket ticket, Guid clientId)
    {
        var unreadCount = clientId != Guid.Empty && !IsAdmin() 
            ? _ticketService.GetUnreadMessageCountAsync(ticket.Id, clientId).Result 
            : 0;

        return new TicketDto
        {
            Id = ticket.Id,
            ClientId = ticket.ClientId,
            Subject = ticket.Subject,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Category = ticket.Category,
            AssignedToAdminId = ticket.AssignedToAdminId,
            OrderId = ticket.OrderId,
            DesignId = ticket.DesignId,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ClosedAt = ticket.ClosedAt,
            Messages = ticket.Messages?.Select(MapToTicketMessageDto).OrderBy(m => m.CreatedAt).ToList() ?? new List<TicketMessageDto>(),
            UnreadMessageCount = unreadCount
        };
    }

    private static TicketMessageDto MapToTicketMessageDto(TicketMessage message)
    {
        return new TicketMessageDto
        {
            Id = message.Id,
            TicketId = message.TicketId,
            AuthorId = message.AuthorId,
            AuthorName = message.AuthorName,
            Content = message.Content,
            IsFromAdmin = message.IsFromAdmin,
            IsReadByClient = message.IsReadByClient,
            CreatedAt = message.CreatedAt
        };
    }
}