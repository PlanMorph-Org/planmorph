using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Application.DTOs.Order;
using PlanMorph.Application.Services;
using System.Security.Claims;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        if (createOrderDto.PaymentMethod != PlanMorph.Core.Entities.PaymentMethod.Paystack)
            return BadRequest(new { message = "Paystack is the only supported payment method at the moment." });

        if (createOrderDto.IncludesConstruction)
        {
            if (string.IsNullOrWhiteSpace(createOrderDto.ConstructionLocation))
                return BadRequest(new { message = "Construction location is required for construction services." });

            if (!string.Equals(createOrderDto.ConstructionCountry, "Kenya", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Construction services are currently available only in Kenya." });
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await _orderService.CreateOrderAsync(createOrderDto, userId);

        if (order == null)
            return BadRequest(new { message = "Failed to create order. Design may not be available." });

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        var order = await _orderService.GetOrderByIdAsync(id, userId);

        // Allow admins to view any order
        if (order == null && userRole != "Admin")
            return NotFound();

        return Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}/files")]
    public async Task<IActionResult> GetOrderFiles(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole != "Client")
            return Forbid();

        var files = await _orderService.GetOrderFilesAsync(id, userId);
        if (files == null)
            return NotFound();

        return Ok(files);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromBody] MarkAsPaidDto dto)
    {
        var result = await _orderService.MarkOrderAsPaidAsync(id, dto.PaymentReference);

        if (!result)
            return NotFound();

        return Ok(new { message = "Order marked as paid successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/mark-completed")]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        var result = await _orderService.MarkOrderAsCompletedAsync(id);

        if (!result)
            return BadRequest(new { message = "Only paid orders can be marked as completed." });

        return Ok(new { message = "Order marked as completed successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/construction-contract")]
    public async Task<IActionResult> CreateConstructionContract(Guid id, [FromBody] CreateConstructionContractDto contractDto)
    {
        var result = await _orderService.CreateConstructionContractAsync(id, contractDto);

        if (!result)
            return BadRequest(new { message = "Failed to create construction contract. Order may not exist or not be paid." });

        return Ok(new { message = "Construction contract created successfully" });
    }

    [HttpPost("{id}/request-construction")]
    public async Task<IActionResult> RequestConstruction(Guid id, [FromBody] RequestConstructionDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Location))
            return BadRequest(new { message = "Construction location is required." });

        if (!string.Equals(requestDto.Country, "Kenya", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Construction services are currently available only in Kenya." });

        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userRole != "Client")
            return Forbid();

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _orderService.RequestConstructionAsync(id, userId, requestDto);

        if (!result)
            return BadRequest(new { message = "Unable to submit construction request." });

        return Ok(new { message = "Construction request submitted successfully." });
    }
}
