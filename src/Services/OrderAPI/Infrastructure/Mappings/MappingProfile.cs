using AutoMapper;
using Mango.Services.OrderAPI.Domain;
using Mango.Services.OrderAPI.Application.DTOs;

namespace Mango.Services.OrderAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Order API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>().ReverseMap();
        
        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>().ReverseMap();
    }
}
