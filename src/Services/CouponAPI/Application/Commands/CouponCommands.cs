using MediatR;
using Mango.Services.CouponAPI.Application.DTOs;
using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Application.Commands;

/// <summary>
/// Create Coupon Command
/// </summary>
public record CreateCouponCommand(
    string Code,
    string Description,
    CouponType CouponType,
    decimal DiscountAmount,
    decimal DiscountPercentage,
    decimal MinOrderAmount,
    int MaxUsageCount,
    DateTime ValidFrom,
    DateTime ValidUntil
) : IRequest<CouponDto>;

/// <summary>
/// Update Coupon Command
/// </summary>
public record UpdateCouponCommand(
    Guid Id,
    string Code,
    string Description,
    CouponType CouponType,
    decimal DiscountAmount,
    decimal DiscountPercentage,
    decimal MinOrderAmount,
    int MaxUsageCount,
    DateTime ValidFrom,
    DateTime ValidUntil,
    bool IsActive
) : IRequest<CouponDto?>;

/// <summary>
/// Delete Coupon Command
/// </summary>
public record DeleteCouponCommand(Guid Id) : IRequest<bool>;

/// <summary>
/// Validate Coupon Command
/// </summary>
public record ValidateCouponCommand(
    string Code,
    decimal OrderAmount
) : IRequest<CouponValidationResultDto>;
