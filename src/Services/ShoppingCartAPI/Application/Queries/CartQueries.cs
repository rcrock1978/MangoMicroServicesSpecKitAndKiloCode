using MediatR;
using Mango.Services.ShoppingCartAPI.Application.DTOs;

namespace Mango.Services.ShoppingCartAPI.Application.Queries;

/// <summary>
/// Get Cart By User ID Query
/// </summary>
public record GetCartByUserIdQuery(string UserId) : IRequest<CartDto?>;
