using MediatR;
using AutoMapper;
using Mango.Services.ShoppingCartAPI.Application.Commands;
using Mango.Services.ShoppingCartAPI.Application.DTOs;
using Mango.Services.ShoppingCartAPI.Application.Queries;
using Mango.Services.ShoppingCartAPI.Domain;
using Mango.Services.ShoppingCartAPI.Domain.Interfaces;

namespace Mango.Services.ShoppingCartAPI.Application.Handlers;

/// <summary>
/// Add to Cart Handler
/// </summary>
public class AddToCartHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public AddToCartHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(request.UserId);
        
        if (cart == null)
        {
            // Create new cart
            cart = new Cart
            {
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };
            
            var cartItem = new CartItem
            {
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                Price = request.Price,
                Quantity = request.Quantity,
                ImageUrl = request.ImageUrl
            };
            
            cart.Items.Add(cartItem);
            cart.Total = cartItem.Price * cartItem.Quantity;
            
            await _repository.AddAsync(cart);
        }
        else
        {
            // Check if item already exists
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    Price = request.Price,
                    Quantity = request.Quantity,
                    ImageUrl = request.ImageUrl
                });
            }
            
            // Recalculate total
            cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
            cart.UpdatedAt = DateTime.UtcNow;
            
            await _repository.UpdateAsync(cart);
        }
        
        return _mapper.Map<CartDto>(cart);
    }
}

/// <summary>
/// Update Cart Item Handler
/// </summary>
public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, CartDto?>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public UpdateCartItemHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(request.UserId);
        if (cart == null)
            return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item == null)
            return null;

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }
}

/// <summary>
/// Remove Cart Item Handler
/// </summary>
public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, bool>
{
    private readonly ICartRepository _repository;

    public RemoveCartItemHandler(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(request.UserId);
        if (cart == null)
            return false;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item == null)
            return false;

        cart.Items.Remove(item);
        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(cart);
        return true;
    }
}

/// <summary>
/// Clear Cart Handler
/// </summary>
public class ClearCartHandler : IRequestHandler<ClearCartCommand, bool>
{
    private readonly ICartRepository _repository;

    public ClearCartHandler(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteByUserIdAsync(request.UserId);
        return true;
    }
}

/// <summary>
/// Apply Coupon Handler
/// </summary>
public class ApplyCouponHandler : IRequestHandler<ApplyCouponCommand, CartDto>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public ApplyCouponHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(request.UserId);
        if (cart == null)
            throw new Exception("Cart not found");

        cart.CouponCode = request.CouponCode;
        cart.Discount = request.Discount;
        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity) - request.Discount;
        cart.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }
}

/// <summary>
/// Get Cart By User ID Handler
/// </summary>
public class GetCartByUserIdHandler : IRequestHandler<GetCartByUserIdQuery, CartDto?>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public GetCartByUserIdHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByUserIdWithItemsAsync(request.UserId);
        if (cart == null)
            return null;
        
        return _mapper.Map<CartDto>(cart);
    }
}
