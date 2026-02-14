using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces.Services;

public interface ITicketNotificationService
{
    Task SendTicketCreatedNotificationAsync(Ticket ticket);
    Task SendTicketStatusChangedNotificationAsync(Ticket ticket, TicketStatus previousStatus);
    Task SendTicketAssignedNotificationAsync(Ticket ticket, string adminName);
    Task SendNewMessageNotificationAsync(Ticket ticket, TicketMessage message);
    Task SendTicketClosedNotificationAsync(Ticket ticket);
    Task SendTicketReopenedNotificationAsync(Ticket ticket);
}