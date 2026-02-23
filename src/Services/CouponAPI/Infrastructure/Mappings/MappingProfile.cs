using AutoMapper;
using Mango.Services.CouponAPI.Domain;
using Mango.Services.CouponAPI.Application.DTOs;

namespace Mango.Services.CouponAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Coupon API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Coupon mappings
        CreateMap<Coupon, CouponDto>().ReverseMap();
    }
}
