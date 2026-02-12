using PlanMorph.Application.DTOs.Order;

namespace PlanMorph.Application.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid clientId);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
    Task<bool> MarkOrderAsPaidAsync(Guid orderId, string paymentReference);
    Task<bool> MarkOrderAsCompletedAsync(Guid orderId);
    Task<bool> CreateConstructionContractAsync(Guid orderId, CreateConstructionContractDto contractDto);
    Task<bool> RequestConstructionAsync(Guid orderId, Guid userId, RequestConstructionDto requestDto);
    Task<IReadOnlyList<OrderFileDto>?> GetOrderFilesAsync(Guid orderId, Guid userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(); // Admin only
}
