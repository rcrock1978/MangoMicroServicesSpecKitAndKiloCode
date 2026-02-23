using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Domain;
using Mango.Services.OrderAPI.Domain.Interfaces;
using Mango.Services.OrderAPI.Infrastructure.Data;

namespace Mango.Services.OrderAPI.Infrastructure.Repositories;

/// <summary>
/// Order Repository Implementation
/// </summary>
public class OrderRepository : IOrderRepository
{
    protected readonly OrderDbContext _context;
    protected readonly DbSet<Order> _dbSet;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Order>();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllAsync(int page, int pageSize)
    {
        return await _dbSet
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Order> AddAsync(Order entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Order entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}
