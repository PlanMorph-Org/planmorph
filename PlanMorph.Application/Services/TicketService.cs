using Microsoft.EntityFrameworkCore;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Services;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketNotificationService _notificationService;

    public TicketService(IUnitOfWork unitOfWork, ITicketNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Ticket> CreateTicketAsync(Guid clientId, string subject, string description, 
        TicketCategory category, TicketPriority priority, Guid? orderId = null, Guid? designId = null)
    {
        // Generate ticket number: TKT-YYYYMMDD-XXXX
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"TKT-{date}-";
        var todayTickets = await _unitOfWork.Tickets.FindAsync(t => t.TicketNumber.StartsWith(prefix));
        var sequence = todayTickets.Count() + 1;

        var ticket = new Ticket
        {
            TicketNumber = $"{prefix}{sequence:D4}",
            ClientId = clientId,
            Subject = subject,
            Description = description,
            Category = category,
            Priority = priority,
            Status = TicketStatus.Open,
            OrderId = orderId,
            DesignId = designId
        };

        // Add initial message with description
        var initialMessage = new TicketMessage
        {
            TicketId = ticket.Id,
            AuthorId = clientId,
            AuthorName = "Client", // This could be enhanced to get actual client name
            Content = description,
            IsFromAdmin = false,
            IsReadByClient = true
        };

        ticket.Messages.Add(initialMessage);

        await _unitOfWork.TicketRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Send notification
        await _notificationService.SendTicketCreatedNotificationAsync(ticket);

        return ticket;
    }

    public async Task<IEnumerable<Ticket>> GetClientTicketsAsync(Guid clientId)
    {
        return await _unitOfWork.TicketRepository.GetTicketsByClientIdAsync(clientId);
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId, Guid clientId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetTicketWithMessagesAsync(ticketId);
        
        // Ensure client can only access their own tickets
        if (ticket?.ClientId != clientId)
            return null;

        return ticket;
    }

    public async Task<bool> AddMessageAsync(Guid ticketId, Guid authorId, string content, bool isFromAdmin = false)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        // If ticket is closed, reopen it when client sends a message
        if (!isFromAdmin && ticket.Status == TicketStatus.Closed)
        {
            var previousStatus = ticket.Status;
            ticket.Status = TicketStatus.Open;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _notificationService.SendTicketReopenedNotificationAsync(ticket);
        }

        var message = new TicketMessage
        {
            TicketId = ticketId,
            AuthorId = authorId,
            AuthorName = isFromAdmin ? "Support Team" : "Client",
            Content = content,
            IsFromAdmin = isFromAdmin,
            IsReadByClient = isFromAdmin ? false : true
        };

        await _unitOfWork.TicketMessageRepository.AddAsync(message);
        
        // Update ticket's last activity
        ticket.UpdatedAt = DateTime.UtcNow;
        
        var result = await _unitOfWork.SaveChangesAsync() > 0;

        if (result)
        {
            // Send notification
            await _notificationService.SendNewMessageNotificationAsync(ticket, message);
        }

        return result;
    }

    public async Task<bool> CloseTicketAsync(Guid ticketId, Guid clientId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(ticketId);
        if (ticket == null || ticket.ClientId != clientId) return false;

        var previousStatus = ticket.Status;
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        var result = await _unitOfWork.SaveChangesAsync() > 0;

        if (result)
        {
            await _notificationService.SendTicketClosedNotificationAsync(ticket);
        }

        return result;
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid ticketId, Guid clientId)
    {
        return await _unitOfWork.TicketMessageRepository.GetUnreadMessageCountByTicketAsync(ticketId, clientId);
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid ticketId, Guid clientId)
    {
        return await _unitOfWork.TicketMessageRepository.MarkMessagesAsReadAsync(ticketId, clientId);
    }

    // Admin operations
    public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
    {
        return await _unitOfWork.TicketRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
    {
        return await _unitOfWork.TicketRepository.GetTicketsByStatusAsync(status);
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority)
    {
        return await _unitOfWork.TicketRepository.GetTicketsByPriorityAsync(priority);
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByCategoryAsync(TicketCategory category)
    {
        return await _unitOfWork.TicketRepository.GetTicketsByCategoryAsync(category);
    }

    public async Task<IEnumerable<Ticket>> GetAssignedTicketsAsync(Guid adminId)
    {
        return await _unitOfWork.TicketRepository.GetTicketsAssignedToAdminAsync(adminId);
    }

    public async Task<Ticket?> GetTicketWithMessagesAsync(Guid ticketId)
    {
        return await _unitOfWork.TicketRepository.GetTicketWithMessagesAsync(ticketId);
    }

    public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status, Guid adminId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        var previousStatus = ticket.Status;
        var result = await _unitOfWork.TicketRepository.UpdateTicketStatusAsync(ticketId, status);

        if (result && previousStatus != status)
        {
            await _notificationService.SendTicketStatusChangedNotificationAsync(ticket, previousStatus);
        }

        return result;
    }

    public async Task<bool> AssignTicketAsync(Guid ticketId, Guid adminId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        var result = await _unitOfWork.TicketRepository.AssignTicketToAdminAsync(ticketId, adminId);

        if (result)
        {
            // Get admin name for notification (this would need to be enhanced)
            await _notificationService.SendTicketAssignedNotificationAsync(ticket, "Support Team");
        }

        return result;
    }

    public async Task<bool> UpdateTicketPriorityAsync(Guid ticketId, TicketPriority priority, Guid adminId)
    {
        var ticket = await _unitOfWork.TicketRepository.GetByIdAsync(ticketId);
        if (ticket == null) return false;

        ticket.Priority = priority;
        ticket.UpdatedAt = DateTime.UtcNow;

        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Ticket>> SearchTicketsAsync(string searchTerm)
    {
        return await _unitOfWork.TicketRepository.SearchTicketsAsync(searchTerm);
    }

    public async Task<Dictionary<TicketStatus, int>> GetTicketStatusStatsAsync()
    {
        var tickets = await _unitOfWork.TicketRepository.GetAllAsync();
        return tickets.GroupBy(t => t.Status)
                     .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<TicketPriority, int>> GetTicketPriorityStatsAsync()
    {
        var tickets = await _unitOfWork.TicketRepository.GetAllAsync();
        return tickets.GroupBy(t => t.Priority)
                     .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<TicketCategory, int>> GetTicketCategoryStatsAsync()
    {
        var tickets = await _unitOfWork.TicketRepository.GetAllAsync();
        return tickets.GroupBy(t => t.Category)
                     .ToDictionary(g => g.Key, g => g.Count());
    }
}