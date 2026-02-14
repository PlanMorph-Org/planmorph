using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Repositories;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Infrastructure.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByClientIdAsync(Guid clientId)
    {
        return await _context.Tickets
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
    {
        return await _context.Tickets
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority)
    {
        return await _context.Tickets
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category)
    {
        return await _context.Tickets
            .Where(t => t.Category == category)
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsAssignedToAdminAsync(Guid adminId)
    {
        return await _context.Tickets
            .Where(t => t.AssignedToAdminId == adminId)
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketWithMessagesAsync(Guid ticketId)
    {
        return await _context.Tickets
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<IEnumerable<Ticket>> GetRecentTicketsAsync(int count = 10)
    {
        return await _context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<int> GetOpenTicketCountByClientAsync(Guid clientId)
    {
        return await _context.Tickets
            .CountAsync(t => t.ClientId == clientId && t.Status != TicketStatus.Closed);
    }

    public async Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm)
    {
        return await _context.Tickets
            .Where(t => t.Subject.Contains(searchTerm) || 
                       t.Description.Contains(searchTerm) ||
                       t.Messages.Any(m => m.Content.Contains(searchTerm)))
            .OrderByDescending(t => t.CreatedAt)
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .ToListAsync();
    }

    public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (status == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AssignTicketToAdminAsync(Guid ticketId, Guid adminId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return false;

        ticket.AssignedToAdminId = adminId;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (ticket.Status == TicketStatus.Open)
        {
            ticket.Status = TicketStatus.Assigned;
        }

        return await _context.SaveChangesAsync() > 0;
    }
}