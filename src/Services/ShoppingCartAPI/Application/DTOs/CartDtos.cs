namespace Mango.Services.ShoppingCartAPI.Application.DTOs;

/// <summary>
/// Cart DTO
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

/// <summary>
/// Cart Item DTO
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Request DTOs
/// </summary>
public class AddToCartRequest
{
    public string UserId { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ApplyCouponRequest
{
    public string UserId { get; set; } = string.Empty;
    public string CouponCode { get; set; } = string.Empty;
}
