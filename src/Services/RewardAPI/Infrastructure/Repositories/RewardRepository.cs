using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Domain;
using Mango.Services.RewardAPI.Domain.Interfaces;
using Mango.Services.RewardAPI.Infrastructure.Data;

namespace Mango.Services.RewardAPI.Infrastructure.Repositories;

/// <summary>
/// User Reward Repository Implementation
/// </summary>
public class UserRewardRepository : IUserRewardRepository
{
    protected readonly RewardDbContext _context;
    protected readonly DbSet<UserReward> _dbSet;

    public UserRewardRepository(RewardDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserReward>();
    }

    public async Task<UserReward?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<UserReward?> GetByUserIdWithTransactionsAsync(string userId)
    {
        return await _dbSet
            .Include(u => u.Transactions.OrderByDescending(t => t.CreatedAt))
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<UserReward> AddAsync(UserReward entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(UserReward entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// Reward Repository Implementation
/// </summary>
public class RewardRepository : IRewardRepository
{
    protected readonly RewardDbContext _context;
    protected readonly DbSet<Reward> _dbSet;

    public RewardRepository(RewardDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Reward>();
    }

    public async Task<Reward?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<Reward>> GetAllAsync()
    {
        return await _dbSet.Where(r => r.IsActive).ToListAsync();
    }

    public async Task<Reward> AddAsync(Reward entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Reward entity)
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
