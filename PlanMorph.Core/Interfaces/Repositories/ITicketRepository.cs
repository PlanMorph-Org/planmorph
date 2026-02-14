using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces.Repositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetTicketsByClientIdAsync(Guid clientId);
    Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status);
    Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority);
    Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category);
    Task<IEnumerable<Ticket>> GetTicketsAssignedToAdminAsync(Guid adminId);
    Task<Ticket?> GetTicketWithMessagesAsync(Guid ticketId);
    Task<IEnumerable<Ticket>> GetRecentTicketsAsync(int count = 10);
    Task<int> GetOpenTicketCountByClientAsync(Guid clientId);
    Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm);
    Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status);
    Task<bool> AssignTicketToAdminAsync(Guid ticketId, Guid adminId);
}