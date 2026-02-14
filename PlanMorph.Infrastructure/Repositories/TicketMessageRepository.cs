using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Repositories;
using PlanMorph.Infrastructure.Data;

namespace PlanMorph.Infrastructure.Repositories;

public class TicketMessageRepository : Repository<TicketMessage>, ITicketMessageRepository
{
    public TicketMessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TicketMessage>> GetMessagesByTicketIdAsync(Guid ticketId)
    {
        return await _context.TicketMessages
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<TicketMessage?> GetLastMessageAsync(Guid ticketId)
    {
        return await _context.TicketMessages
            .Where(m => m.TicketId == ticketId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TicketMessage>> GetMessagesByAuthorAsync(Guid authorId)
    {
        return await _context.TicketMessages
            .Where(m => m.AuthorId == authorId)
            .OrderByDescending(m => m.CreatedAt)
            .Include(m => m.Ticket)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessageCountByTicketAsync(Guid ticketId, Guid clientId)
    {
        return await _context.TicketMessages
            .Where(m => m.TicketId == ticketId && 
                       m.AuthorId != clientId && 
                       !m.IsReadByClient)
            .CountAsync();
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid ticketId, Guid clientId)
    {
        var messages = await _context.TicketMessages
            .Where(m => m.TicketId == ticketId && 
                       m.AuthorId != clientId && 
                       !m.IsReadByClient)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsReadByClient = true;
        }

        return await _context.SaveChangesAsync() > 0;
    }
}