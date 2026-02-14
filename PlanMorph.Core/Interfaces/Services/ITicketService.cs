using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces.Services;

public interface ITicketService
{
    // Client operations
    Task<Ticket> CreateTicketAsync(Guid clientId, string subject, string description, 
        TicketCategory category, TicketPriority priority, Guid? orderId = null, Guid? designId = null);
    Task<IEnumerable<Ticket>> GetClientTicketsAsync(Guid clientId);
    Task<Ticket?> GetTicketByIdAsync(Guid ticketId, Guid clientId);
    Task<bool> AddMessageAsync(Guid ticketId, Guid authorId, string content, bool isFromAdmin = false);
    Task<bool> CloseTicketAsync(Guid ticketId, Guid clientId);
    Task<int> GetUnreadMessageCountAsync(Guid ticketId, Guid clientId);
    Task<bool> MarkMessagesAsReadAsync(Guid ticketId, Guid clientId);

    // Admin operations  
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status);
    Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority);
    Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category);
    Task<IEnumerable<Ticket>> GetAssignedTicketsAsync(Guid adminId);
    Task<Ticket?> GetTicketWithMessagesAsync(Guid ticketId);
    Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status, Guid adminId);
    Task<bool> AssignTicketAsync(Guid ticketId, Guid adminId);
    Task<bool> UpdateTicketPriorityAsync(Guid ticketId, TicketPriority priority, Guid adminId);
    Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm);
    
    // Statistics
    Task<Dictionary<TicketStatus, int>> GetTicketStatusStatsAsync();
    Task<Dictionary<TicketPriority, int>> GetTicketPriorityStatsAsync();
    Task<Dictionary<TicketCategory, int>> GetTicketCategoryStatsAsync();
}