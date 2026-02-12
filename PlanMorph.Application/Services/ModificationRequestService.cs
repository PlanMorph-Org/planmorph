using PlanMorph.Application.DTOs.Modification;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class ModificationRequestService : IModificationRequestService
{
    private readonly IUnitOfWork _unitOfWork;

    public ModificationRequestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ModificationRequestDto?> CreateRequestAsync(CreateModificationRequestDto dto, Guid clientId)
    {
        // Verify order exists and belongs to client
        var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
        if (order == null || order.ClientId != clientId || order.Status != OrderStatus.Paid)
            return null;

        var request = new ModificationRequest
        {
            OrderId = dto.OrderId,
            DesignId = order.DesignId,
            Description = dto.Description,
            Status = ModificationStatus.Pending
        };

        await _unitOfWork.ModificationRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(request);
    }

    public async Task<ModificationRequestDto?> GetRequestByIdAsync(Guid requestId)
    {
        var request = await _unitOfWork.ModificationRequests.GetByIdAsync(requestId);
        return request == null ? null : await MapToDto(request);
    }

    public async Task<IEnumerable<ModificationRequestDto>> GetRequestsByOrderAsync(Guid orderId)
    {
        var requests = await _unitOfWork.ModificationRequests.FindAsync(r => r.OrderId == orderId);
        
        var dtos = new List<ModificationRequestDto>();
        foreach (var request in requests)
        {
            var dto = await MapToDto(request);
            if (dto != null)
                dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> SubmitQuoteAsync(Guid requestId, decimal quotedPrice)
    {
        var request = await _unitOfWork.ModificationRequests.GetByIdAsync(requestId);
        if (request == null || request.Status != ModificationStatus.Pending)
            return false;

        request.QuotedPrice = quotedPrice;
        request.Status = ModificationStatus.Quoted;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ModificationRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AcceptQuoteAsync(Guid requestId)
    {
        var request = await _unitOfWork.ModificationRequests.GetByIdAsync(requestId);
        if (request == null || request.Status != ModificationStatus.Quoted)
            return false;

        request.Status = ModificationStatus.Accepted;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ModificationRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectRequestAsync(Guid requestId)
    {
        var request = await _unitOfWork.ModificationRequests.GetByIdAsync(requestId);
        if (request == null)
            return false;

        request.Status = ModificationStatus.Rejected;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ModificationRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsCompletedAsync(Guid requestId)
    {
        var request = await _unitOfWork.ModificationRequests.GetByIdAsync(requestId);
        if (request == null || request.Status != ModificationStatus.Accepted)
            return false;

        request.Status = ModificationStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ModificationRequests.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<ModificationRequestDto?> MapToDto(ModificationRequest request)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(request.DesignId);
        if (design == null)
            return null;

        return new ModificationRequestDto
        {
            Id = request.Id,
            OrderId = request.OrderId,
            DesignId = request.DesignId,
            DesignTitle = design.Title,
            Description = request.Description,
            QuotedPrice = request.QuotedPrice,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt,
            CompletedAt = request.CompletedAt
        };
    }
}