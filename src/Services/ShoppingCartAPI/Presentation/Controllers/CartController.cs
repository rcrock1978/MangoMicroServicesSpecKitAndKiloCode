using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.ShoppingCartAPI.Application.Commands;
using Mango.Services.ShoppingCartAPI.Application.DTOs;
using Mango.Services.ShoppingCartAPI.Application.Queries;

namespace Mango.Services.ShoppingCartAPI.Presentation.Controllers;

/// <summary>
/// Cart controller
/// </summary>
[ApiController]
[Route("api/v1/cart")]
[ApiVersion("1.0")]
public class CartController : ControllerBase
{
    private readonly ILogger<CartController> _logger;
    private readonly IMediator _mediator;

    public CartController(ILogger<CartController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get cart by user ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(string userId)
    {
        _logger.LogInformation("GetCart called for user: {UserId}", userId);
        
        var query = new GetCartByUserIdQuery(userId);
        var cart = await _mediator.Send(query);
        
        if (cart == null)
            return NotFound(new { message = "Cart not found" });
        
        return Ok(cart);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        _logger.LogInformation("AddToCart called for user: {UserId}, product: {ProductId}", 
            request.UserId, request.ProductId);
        
        var command = new AddToCartCommand(
            request.UserId,
            request.ProductId,
            request.ProductName,
            request.Price,
            request.Quantity,
            request.ImageUrl
        );
        
        var cart = await _mediator.Send(command);
        
        return Ok(cart);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("{userId}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCartItem(string userId, [FromBody] UpdateCartItemRequest request)
    {
        _logger.LogInformation("UpdateCartItem called for user: {UserId}, product: {ProductId}", 
            userId, request.ProductId);
        
        var command = new UpdateCartItemCommand(userId, request.ProductId, request.Quantity);
        var cart = await _mediator.Send(command);
        
        if (cart == null)
            return NotFound(new { message = "Cart or item not found" });
        
        return Ok(cart);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCartItem(string userId, Guid productId)
    {
        _logger.LogInformation("RemoveCartItem called for user: {UserId}, product: {ProductId}", 
            userId, productId);
        
        var command = new RemoveCartItemCommand(userId, productId);
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound(new { message = "Item not found" });
        
        return Ok(new { message = "Item removed successfully" });
    }

    /// <summary>
    /// Clear cart
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCart(string userId)
    {
        _logger.LogInformation("ClearCart called for user: {UserId}", userId);
        
        var command = new ClearCartCommand(userId);
        await _mediator.Send(command);
        
        return Ok(new { message = "Cart cleared successfully" });
    }

    /// <summary>
    /// Apply coupon
    /// </summary>
    [HttpPost("{userId}/coupon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyCoupon(string userId, [FromBody] ApplyCouponRequest request)
    {
        _logger.LogInformation("ApplyCoupon called for user: {UserId}, coupon: {CouponCode}", 
            userId, request.CouponCode);
        
        var command = new ApplyCouponCommand(userId, request.CouponCode, 0); // Discount would be fetched from Coupon service
        var cart = await _mediator.Send(command);
        
        return Ok(cart);
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "CartAPI" });
    }
}
