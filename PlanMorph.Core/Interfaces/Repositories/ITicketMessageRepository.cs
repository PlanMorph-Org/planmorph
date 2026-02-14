using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces.Repositories;

public interface ITicketMessageRepository : IRepository<TicketMessage>
{
    Task<IEnumerable<TicketMessage>> GetMessagesByTicketIdAsync(Guid ticketId);
    Task<TicketMessage?> GetLastMessageAsync(Guid ticketId);  
    Task<IEnumerable<TicketMessage>> GetMessagesByAuthorAsync(Guid authorId);
    Task<int> GetUnreadMessageCountByTicketAsync(Guid ticketId, Guid clientId);
    Task<bool> MarkMessagesAsReadAsync(Guid ticketId, Guid clientId);
}