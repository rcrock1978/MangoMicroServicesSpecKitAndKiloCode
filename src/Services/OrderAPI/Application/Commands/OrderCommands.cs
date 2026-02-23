using MediatR;
using Mango.Services.OrderAPI.Application.DTOs;
using Mango.Services.OrderAPI.Domain;

namespace Mango.Services.OrderAPI.Application.Commands;

/// <summary>
/// Create Order Command
/// </summary>
public record CreateOrderCommand(
    string UserId,
    string UserName,
    string UserEmail,
    string ShippingAddress,
    string ShippingCity,
    string ShippingState,
    string ShippingZipCode,
    string ShippingCountry,
    string PaymentMethod,
    List<CreateOrderItemRequest> Items
) : IRequest<OrderDto>;

/// <summary>
/// Update Order Status Command
/// </summary>
public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status,
    string? CancellationReason
) : IRequest<OrderDto?>;

///// <summary>
///// Cancel Order Command
///// </summary>
//public record CancelOrderCommand(
//    Guid OrderId,
//    string Reason
//) : IRequest<bool>;
