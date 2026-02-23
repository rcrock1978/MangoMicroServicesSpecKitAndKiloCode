using MediatR;
using Mango.Services.OrderAPI.Application.DTOs;

namespace Mango.Services.OrderAPI.Application.Queries;

/// <summary>
/// Get Order By ID Query
/// </summary>
public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;

/// <summary>
/// Get Orders By User ID Query
/// </summary>
public record GetOrdersByUserIdQuery(string UserId) : IRequest<List<OrderDto>>;

/// <summary>
/// Get All Orders Query
/// </summary>
public record GetOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<OrderDto>>;

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
