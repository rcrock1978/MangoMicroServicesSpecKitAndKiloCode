using AutoMapper;
using Mango.Services.ShoppingCartAPI.Domain;
using Mango.Services.ShoppingCartAPI.Application.DTOs;

namespace Mango.Services.ShoppingCartAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Shopping Cart API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Cart mappings
        CreateMap<Cart, CartDto>().ReverseMap();
        
        // CartItem mappings
        CreateMap<CartItem, CartItemDto>().ReverseMap();
    }
}
