using MediatR;
using Mango.Services.ShoppingCartAPI.Application.DTOs;

namespace Mango.Services.ShoppingCartAPI.Application.Commands;

/// <summary>
/// Add to Cart Command
/// </summary>
public record AddToCartCommand(
    string UserId,
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    string? ImageUrl
) : IRequest<CartDto>;

/// <summary>
/// Update Cart Item Command
/// </summary>
public record UpdateCartItemCommand(
    string UserId,
    Guid ProductId,
    int Quantity
) : IRequest<CartDto?>;

/// <summary>
/// Remove Cart Item Command
/// </summary>
public record RemoveCartItemCommand(
    string UserId,
    Guid ProductId
) : IRequest<bool>;

/// <summary>
/// Clear Cart Command
/// </summary>
public record ClearCartCommand(string UserId) : IRequest<bool>;

/// <summary>
/// Apply Coupon Command
/// </summary>
public record ApplyCouponCommand(
    string UserId,
    string CouponCode,
    decimal Discount
) : IRequest<CartDto>;
