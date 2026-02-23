using MediatR;
using Mango.Services.ProductAPI.Application.DTOs;

namespace Mango.Services.ProductAPI.Application.Commands;

/// <summary>
/// Create Product Command
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string CategoryName,
    string? ImageUrl,
    int StockQuantity
) : IRequest<ProductDto>;

/// <summary>
/// Update Product Command
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string CategoryName,
    string? ImageUrl,
    int StockQuantity,
    bool IsActive
) : IRequest<ProductDto?>;

/// <summary>
/// Delete Product Command
/// </summary>
public record DeleteProductCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Create Category Command
/// </summary>
public record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<CategoryDto>;

/// <summary>
/// Update Stock Command
/// </summary>
public record UpdateStockCommand(
    Guid ProductId,
    int QuantityChange
) : IRequest<bool>;
