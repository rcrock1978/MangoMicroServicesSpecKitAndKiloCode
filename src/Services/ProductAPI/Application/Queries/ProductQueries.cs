using MediatR;
using Mango.Services.ProductAPI.Application.DTOs;

namespace Mango.Services.ProductAPI.Application.Queries;

/// <summary>
/// Get Product By ID Query
/// </summary>
public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

/// <summary>
/// Get All Products Query
/// </summary>
public record GetProductsQuery(
    string? SearchTerm = null,
    string? CategoryName = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<ProductDto>>;

/// <summary>
/// Get Category By ID Query
/// </summary>
public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

/// <summary>
/// Get All Categories Query
/// </summary>
public record GetCategoriesQuery() : IRequest<List<CategoryDto>>;

/// <summary>
/// Paged Result
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
