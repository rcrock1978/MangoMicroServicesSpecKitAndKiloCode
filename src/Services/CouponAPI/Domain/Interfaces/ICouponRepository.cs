using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Domain.Interfaces;

/// <summary>
/// Coupon Repository Interface
/// </summary>
public interface ICouponRepository
{
    Task<Coupon?> GetByIdAsync(Guid id);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetAllAsync();
    Task<Coupon> AddAsync(Coupon entity);
    Task UpdateAsync(Coupon entity);
    Task DeleteAsync(Guid id);
}
