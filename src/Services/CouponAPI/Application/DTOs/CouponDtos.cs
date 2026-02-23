using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Application.DTOs;

/// <summary>
/// Coupon DTO
/// </summary>
public class CouponDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType CouponType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public int UsedCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTOs
/// </summary>
public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType CouponType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
}

public class UpdateCouponRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType CouponType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Validation result DTO
/// </summary>
public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string Code { get; set; } = string.Empty;
}
