using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Application.DTOs.Order;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    public OrderService(IUnitOfWork unitOfWork, IEmailService emailService, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto createOrderDto, Guid clientId)
    {
        if (createOrderDto.PaymentMethod != PaymentMethod.Paystack)
            return null;

        // Get design
        var design = await _unitOfWork.Designs.GetByIdAsync(createOrderDto.DesignId);
        if (design == null || design.Status != DesignStatus.Approved)
            return null;

        if (createOrderDto.IncludesConstruction)
        {
            if (string.IsNullOrWhiteSpace(createOrderDto.ConstructionLocation))
                return null;

            if (!IsKenyaCountry(createOrderDto.ConstructionCountry))
                return null;
        }

        // Generate order number
        var orderNumber = GenerateOrderNumber();

        // Create order
        var order = new Order
        {
            OrderNumber = orderNumber,
            ClientId = clientId,
            DesignId = createOrderDto.DesignId,
            Amount = design.Price,
            Status = OrderStatus.Pending,
            PaymentMethod = createOrderDto.PaymentMethod,
            IncludesConstruction = createOrderDto.IncludesConstruction
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // If construction is included, create contract placeholder and send notification emails
        if (createOrderDto.IncludesConstruction && !string.IsNullOrEmpty(createOrderDto.ConstructionLocation))
        {
            var location = NormalizeKenyaLocation(createOrderDto.ConstructionLocation, createOrderDto.ConstructionCountry);
            var contract = new ConstructionContract
            {
                OrderId = order.Id,
                Location = location,
                EstimatedCost = design.EstimatedConstructionCost,
                CommissionAmount = design.EstimatedConstructionCost * 0.02m, // 2% commission
                Status = ContractStatus.Pending,
                ContractorId = null // Will be assigned by admin
            };

            await _unitOfWork.ConstructionContracts.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            // Send email to client confirming construction request received
            var client = await _userManager.FindByIdAsync(clientId.ToString());
            if (client?.Email != null)
            {
                await _emailService.SendConstructionRequestReceivedEmailAsync(
                    client.Email,
                    $"{client.FirstName} {client.LastName}",
                    design.Title,
                    location
                );
            }

            // Send email to admin notifying of new construction request
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmail = admins.FirstOrDefault()?.Email;
            if (!string.IsNullOrEmpty(adminEmail))
            {
                await _emailService.SendAdminConstructionRequestNotificationAsync(
                    adminEmail,
                    $"{client?.FirstName} {client?.LastName}",
                    design.Title,
                    location,
                    order.OrderNumber
                );
            }
        }

        return await MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        
        if (order == null)
            return null;

        // Check if user owns this order (or is admin - we'll handle that in controller)
        if (order.ClientId != userId)
            return null;

        return await MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o => o.ClientId == userId);
        
        var orderDtos = new List<OrderDto>();
        foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
        {
            var dto = await MapToDto(order);
            if (dto != null)
                orderDtos.Add(dto);
        }

        return orderDtos;
    }

    public async Task<bool> MarkOrderAsPaidAsync(Guid orderId, string paymentReference)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return false;

        order.Status = OrderStatus.Paid;
        order.PaymentReference = paymentReference;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Send order confirmation email to client
        var client = await _userManager.FindByIdAsync(order.ClientId.ToString());
        if (client?.Email != null)
        {
            await _emailService.SendOrderConfirmationEmailAsync(
                client.Email,
                $"{client.FirstName} {client.LastName}",
                design.Title,
                order.Amount
            );
        }

        return true;
    }

    public async Task<bool> CreateConstructionContractAsync(Guid orderId, CreateConstructionContractDto contractDto)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null || order.Status != OrderStatus.Paid)
            return false;

        // Check if contract already exists
        var existingContract = await _unitOfWork.ConstructionContracts
            .FirstOrDefaultAsync(c => c.OrderId == orderId);

        if (existingContract != null)
            return false;

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return false;

        var contract = new ConstructionContract
        {
            OrderId = orderId,
            Location = contractDto.Location,
            EstimatedCost = contractDto.EstimatedCost,
            CommissionAmount = contractDto.EstimatedCost * 0.02m,
            Status = ContractStatus.Pending,
            ContractorId = contractDto.ContractorId
        };

        await _unitOfWork.ConstructionContracts.AddAsync(contract);

        order.IncludesConstruction = true;
        await _unitOfWork.Orders.UpdateAsync(order);

        await _unitOfWork.SaveChangesAsync();

        // Send email to client notifying them that a contractor has been assigned
        var client = await _userManager.FindByIdAsync(order.ClientId.ToString());
        if (client?.Email != null && contractDto.ContractorId.HasValue)
        {
            var contractor = await _userManager.FindByIdAsync(contractDto.ContractorId.Value.ToString());
            if (contractor != null)
            {
                await _emailService.SendContractorAssignedToClientEmailAsync(
                    client.Email,
                    $"{client.FirstName} {client.LastName}",
                    design.Title,
                    $"{contractor.FirstName} {contractor.LastName}",
                    contractDto.Location
                );

                // Send email to contractor notifying them of new project assignment
                if (!string.IsNullOrEmpty(contractor.Email))
                {
                    await _emailService.SendContractorAssignmentEmailAsync(
                        contractor.Email,
                        $"{contractor.FirstName} {contractor.LastName}",
                        design.Title,
                        $"{client.FirstName} {client.LastName}",
                        contractDto.Location,
                        contractDto.EstimatedCost
                    );
                }
            }
        }

        return true;
    }

    public async Task<bool> MarkOrderAsCompletedAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            return false;

        if (order.Status != OrderStatus.Paid)
            return false;

        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RequestConstructionAsync(Guid orderId, Guid userId, RequestConstructionDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Location))
            return false;

        if (!IsKenyaCountry(requestDto.Country))
            return false;

        var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null || order.ClientId != userId || order.Status != OrderStatus.Paid)
            return false;

        var existingContract = await _unitOfWork.ConstructionContracts
            .FirstOrDefaultAsync(c => c.OrderId == orderId);

        if (existingContract != null)
            return false;

        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return false;

        var location = NormalizeKenyaLocation(requestDto.Location, requestDto.Country);
        var contract = new ConstructionContract
        {
            OrderId = orderId,
            Location = location,
            EstimatedCost = design.EstimatedConstructionCost,
            CommissionAmount = design.EstimatedConstructionCost * 0.02m,
            Status = ContractStatus.Pending,
            ContractorId = null
        };

        await _unitOfWork.ConstructionContracts.AddAsync(contract);

        order.IncludesConstruction = true;
        await _unitOfWork.Orders.UpdateAsync(order);

        await _unitOfWork.SaveChangesAsync();

        // Send email to client confirming construction request received
        var client = await _userManager.FindByIdAsync(userId.ToString());
        if (client?.Email != null)
        {
            await _emailService.SendConstructionRequestReceivedEmailAsync(
                client.Email,
                $"{client.FirstName} {client.LastName}",
                design.Title,
                location
            );
        }

        // Send email to admin notifying of new construction request
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var adminEmail = admins.FirstOrDefault()?.Email;
        if (!string.IsNullOrEmpty(adminEmail))
        {
            await _emailService.SendAdminConstructionRequestNotificationAsync(
                adminEmail,
                $"{client?.FirstName} {client?.LastName}",
                design.Title,
                location,
                order.OrderNumber
            );
        }

        return true;
    }

    public async Task<IReadOnlyList<OrderFileDto>?> GetOrderFilesAsync(Guid orderId, Guid userId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null || order.ClientId != userId)
            return null;

        if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Completed)
            return null;

        var files = await _unitOfWork.DesignFiles.FindAsync(f =>
            f.DesignId == order.DesignId &&
            f.Category != FileCategory.PreviewImage &&
            f.Category != FileCategory.PreviewVideo);

        return files.Select(f => new OrderFileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            FileType = f.FileType.ToString(),
            Category = f.Category.ToString(),
            FileSizeBytes = f.FileSizeBytes
        }).ToList();
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        
        var orderDtos = new List<OrderDto>();
        foreach (var order in orders.OrderByDescending(o => o.CreatedAt))
        {
            var dto = await MapToDto(order);
            if (dto != null)
                orderDtos.Add(dto);
        }

        return orderDtos;
    }

    private static bool IsKenyaCountry(string? country)
        => string.Equals(country?.Trim(), "Kenya", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeKenyaLocation(string location, string? country)
    {
        var trimmed = location.Trim();
        if (IsKenyaCountry(country) && !trimmed.Contains("Kenya", StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed}, Kenya";
        }

        return trimmed;
    }

    private async Task<OrderDto?> MapToDto(Order order)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(order.DesignId);
        if (design == null)
            return null;

        ConstructionContractDto? contractDto = null;
        
        if (order.IncludesConstruction)
        {
            var contract = await _unitOfWork.ConstructionContracts
                .FirstOrDefaultAsync(c => c.OrderId == order.Id);

            if (contract != null)
            {
                string? contractorName = null;
                if (contract.ContractorId.HasValue)
                {
                    var contractor = await _unitOfWork.Users.GetByIdAsync(contract.ContractorId.Value);
                    contractorName = contractor != null ? $"{contractor.FirstName} {contractor.LastName}" : null;
                }

                contractDto = new ConstructionContractDto
                {
                    Id = contract.Id,
                    Location = contract.Location,
                    EstimatedCost = contract.EstimatedCost,
                    CommissionAmount = contract.CommissionAmount,
                    Status = contract.Status.ToString(),
                    ContractorName = contractorName
                };
            }
        }

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            DesignId = order.DesignId,
            DesignTitle = design.Title,
            Amount = order.Amount,
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            IncludesConstruction = order.IncludesConstruction,
            ConstructionContract = contractDto
        };
    }

    private string GenerateOrderNumber()
    {
        // Format: PM-YYYYMMDD-XXXXX
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(10000, 99999);
        return $"PM-{datePart}-{randomPart}";
    }

    
}

