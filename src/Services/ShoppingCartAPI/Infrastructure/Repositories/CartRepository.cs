using Microsoft.EntityFrameworkCore;
using Mango.Services.ShoppingCartAPI.Domain;
using Mango.Services.ShoppingCartAPI.Domain.Interfaces;
using Mango.Services.ShoppingCartAPI.Infrastructure.Data;

namespace Mango.Services.ShoppingCartAPI.Infrastructure.Repositories;

/// <summary>
/// Cart Repository Implementation
/// </summary>
public class CartRepository : ICartRepository
{
    protected readonly CartDbContext _context;
    protected readonly DbSet<Cart> _dbSet;

    public CartRepository(CartDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Cart>();
    }

    public async Task<Cart?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetByUserIdWithItemsAsync(string userId)
    {
        return await _dbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> AddAsync(Cart entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Cart entity)
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

    public async Task DeleteByUserIdAsync(string userId)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
