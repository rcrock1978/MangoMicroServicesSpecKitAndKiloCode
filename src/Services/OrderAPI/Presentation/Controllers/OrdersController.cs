using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.OrderAPI.Application.Commands;
using Mango.Services.OrderAPI.Application.DTOs;
using Mango.Services.OrderAPI.Application.Queries;
using Mango.Services.OrderAPI.Domain;

namespace Mango.Services.OrderAPI.Presentation.Controllers;

/// <summary>
/// Orders controller
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[ApiVersion("1.0")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IMediator _mediator;

    public OrdersController(ILogger<OrdersController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        _logger.LogInformation("GetOrder called with ID: {OrderId}", orderId);
        
        var query = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(query);
        
        if (order == null)
            return NotFound(new { message = "Order not found" });
        
        return Ok(order);
    }

    /// <summary>
    /// Get orders by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUser(string userId)
    {
        _logger.LogInformation("GetOrdersByUser called for user: {UserId}", userId);
        
        var query = new GetOrdersByUserIdQuery(userId);
        var orders = await _mediator.Send(query);
        
        return Ok(orders);
    }

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GetOrders called with page: {Page}, pageSize: {PageSize}", page, pageSize);
        
        var query = new GetOrdersQuery(page, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("CreateOrder called for user: {UserId}", request.UserId);
        
        var command = new CreateOrderCommand(
            request.UserId,
            request.UserName,
            request.UserEmail,
            request.ShippingAddress,
            request.ShippingCity,
            request.ShippingState,
            request.ShippingZipCode,
            request.ShippingCountry,
            request.PaymentMethod,
            request.Items
        );
        
        var order = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        _logger.LogInformation("UpdateOrderStatus called for order: {OrderId}, status: {Status}", 
            orderId, request.Status);
        
        var command = new UpdateOrderStatusCommand(orderId, request.Status, request.CancellationReason);
        var order = await _mediator.Send(command);
        
        if (order == null)
            return NotFound(new { message = "Order not found" });
        
        return Ok(order);
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "OrderAPI" });
    }
}
