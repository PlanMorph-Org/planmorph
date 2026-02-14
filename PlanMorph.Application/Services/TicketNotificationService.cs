using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces.Services;

namespace PlanMorph.Application.Services;

public class TicketNotificationService : ITicketNotificationService
{
    private readonly IEmailService _emailService;

    public TicketNotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendTicketCreatedNotificationAsync(Ticket ticket)
    {
        // Send confirmation to client
        await _emailService.SendTicketCreatedEmailAsync(
            ticket.ClientId.ToString(), // Convert Guid to string for email lookup
            ticket.Subject,
            ticket.Id.ToString(),
            ticket.Priority.ToString(),
            ticket.Category.ToString()
        );

        // Send notification to admin team
        await _emailService.SendNewTicketAlertEmailAsync(
            ticket.Id.ToString(),
            ticket.Subject,
            ticket.ClientId.ToString(),
            ticket.Priority.ToString(),
            ticket.Category.ToString()
        );
    }

    public async Task SendTicketStatusChangedNotificationAsync(Ticket ticket, TicketStatus previousStatus)
    {
        await _emailService.SendTicketStatusChangedEmailAsync(
            ticket.ClientId.ToString(),
            ticket.Id.ToString(),
            ticket.Subject,
            previousStatus.ToString(),
            ticket.Status.ToString()
        );
    }

    public async Task SendTicketAssignedNotificationAsync(Ticket ticket, string adminName)
    {
        await _emailService.SendTicketAssignedEmailAsync(
            ticket.ClientId.ToString(),
            ticket.Id.ToString(),
            ticket.Subject,
            adminName
        );
    }

    public async Task SendNewMessageNotificationAsync(Ticket ticket, TicketMessage message)
    {
        if (message.IsFromAdmin)
        {
            // Admin sent message, notify client
            await _emailService.SendTicketReplyEmailAsync(
                ticket.ClientId.ToString(),
                ticket.Id.ToString(),
                ticket.Subject,
                message.Content,
                message.AuthorName
            );
        }
        else
        {
            // Client sent message, notify admin team
            await _emailService.SendTicketUpdatedEmailAsync(
                ticket.Id.ToString(),
                ticket.Subject,
                ticket.ClientId.ToString(),
                message.Content
            );
        }
    }

    public async Task SendTicketClosedNotificationAsync(Ticket ticket)
    {
        await _emailService.SendTicketClosedEmailAsync(
            ticket.ClientId.ToString(),
            ticket.Id.ToString(),
            ticket.Subject
        );
    }

    public async Task SendTicketReopenedNotificationAsync(Ticket ticket)
    {
        await _emailService.SendTicketReopenedEmailAsync(
            ticket.ClientId.ToString(),
            ticket.Id.ToString(),
            ticket.Subject
        );
    }
}