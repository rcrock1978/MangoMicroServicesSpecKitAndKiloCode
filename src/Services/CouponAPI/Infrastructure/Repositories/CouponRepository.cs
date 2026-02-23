using Microsoft.EntityFrameworkCore;
using Mango.Services.CouponAPI.Domain;
using Mango.Services.CouponAPI.Domain.Interfaces;
using Mango.Services.CouponAPI.Infrastructure.Data;

namespace Mango.Services.CouponAPI.Infrastructure.Repositories;

/// <summary>
/// Coupon Repository Implementation
/// </summary>
public class CouponRepository : ICouponRepository
{
    protected readonly CouponDbContext _context;
    protected readonly DbSet<Coupon> _dbSet;

    public CouponRepository(CouponDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Coupon>();
    }

    public async Task<Coupon?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<Coupon> AddAsync(Coupon entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Coupon entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
