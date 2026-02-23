using MediatR;
using AutoMapper;
using Mango.Services.OrderAPI.Application.Commands;
using Mango.Services.OrderAPI.Application.DTOs;
using Mango.Services.OrderAPI.Application.Queries;
using Mango.Services.OrderAPI.Domain;
using Mango.Services.OrderAPI.Domain.Interfaces;

namespace Mango.Services.OrderAPI.Application.Handlers;

/// <summary>
/// Create Order Handler
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public CreateOrderHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            UserId = request.UserId,
            UserName = request.UserName,
            UserEmail = request.UserEmail,
            ShippingAddress = request.ShippingAddress,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingZipCode = request.ShippingZipCode,
            ShippingCountry = request.ShippingCountry,
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add items
        foreach (var item in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                TotalPrice = item.Price * item.Quantity
            });
        }

        // Calculate totals
        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);
        order.Discount = 0;
        order.FinalAmount = order.TotalAmount - order.Discount;

        var created = await _repository.AddAsync(order);
        return _mapper.Map<OrderDto>(created);
    }
}

/// <summary>
/// Update Order Status Handler
/// </summary>
public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, OrderDto?>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public UpdateOrderStatusHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdWithItemsAsync(request.OrderId);
        if (order == null)
            return null;

        order.Status = request.Status;
        
        if (request.Status == OrderStatus.Cancelled && !string.IsNullOrEmpty(request.CancellationReason))
        {
            order.CancellationReason = request.CancellationReason;
        }
        
        order.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(order);
        return _mapper.Map<OrderDto>(order);
    }
}

/// <summary>
/// Get Order By ID Handler
/// </summary>
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrderByIdHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdWithItemsAsync(request.OrderId);
        if (order == null)
            return null;
        
        return _mapper.Map<OrderDto>(order);
    }
}

/// <summary>
/// Get Orders By User ID Handler
/// </summary>
public class GetOrdersByUserIdHandler : IRequestHandler<GetOrdersByUserIdQuery, List<OrderDto>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersByUserIdHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<List<OrderDto>>(orders);
    }
}

/// <summary>
/// Get Orders Handler
/// </summary>
public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync(request.Page, request.PageSize);
        var totalCount = await _repository.CountAsync();

        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
