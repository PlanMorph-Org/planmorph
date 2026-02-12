using PlanMorph.Application.DTOs.Modification;

namespace PlanMorph.Application.Services;

public interface IModificationRequestService
{
    Task<ModificationRequestDto?> CreateRequestAsync(CreateModificationRequestDto dto, Guid clientId);
    Task<ModificationRequestDto?> GetRequestByIdAsync(Guid requestId);
    Task<IEnumerable<ModificationRequestDto>> GetRequestsByOrderAsync(Guid orderId);
    Task<bool> SubmitQuoteAsync(Guid requestId, decimal quotedPrice);
    Task<bool> AcceptQuoteAsync(Guid requestId);
    Task<bool> RejectRequestAsync(Guid requestId);
    Task<bool> MarkAsCompletedAsync(Guid requestId);
}