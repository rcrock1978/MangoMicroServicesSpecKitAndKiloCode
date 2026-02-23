using MediatR;
using Mango.Services.CouponAPI.Application.DTOs;

namespace Mango.Services.CouponAPI.Application.Queries;

/// <summary>
/// Get Coupon By ID Query
/// </summary>
public record GetCouponByIdQuery(Guid Id) : IRequest<CouponDto?>;

/// <summary>
/// Get Coupon By Code Query
/// </summary>
public record GetCouponByCodeQuery(string Code) : IRequest<CouponDto?>;

/// <summary>
/// Get All Coupons Query
/// </summary>
public record GetCouponsQuery() : IRequest<List<CouponDto>>;
