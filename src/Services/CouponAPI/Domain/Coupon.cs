namespace Mango.Services.CouponAPI.Domain;

/// <summary>
/// Coupon entity
/// </summary>
public class Coupon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CouponType CouponType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUsageCount { get; set; }
    public int UsedCount { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Coupon Type enumeration
/// </summary>
public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1
}
