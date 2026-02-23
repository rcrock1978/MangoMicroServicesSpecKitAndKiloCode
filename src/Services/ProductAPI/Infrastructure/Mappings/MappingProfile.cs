using AutoMapper;
using Mango.Services.ProductAPI.Domain;
using Mango.Services.ProductAPI.Application.DTOs;

namespace Mango.Services.ProductAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Product API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductDto>().ReverseMap();
        
        // Category mappings
        CreateMap<Category, CategoryDto>().ReverseMap();
    }
}
