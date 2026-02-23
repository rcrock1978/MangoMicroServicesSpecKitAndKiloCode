using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Mango.Services.ProductAPI.Domain;
using Mango.Services.ProductAPI.Domain.Interfaces;
using Mango.Services.ProductAPI.Infrastructure.Data;

namespace Mango.Services.ProductAPI.Infrastructure.Repositories;

/// <summary>
/// Product Repository Implementation
/// </summary>
public class ProductRepository : IProductRepository
{
    protected readonly ProductDbContext _context;
    protected readonly DbSet<Product> _dbSet;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Product>();
    }

    public virtual async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<Product?> GetByIdAsync(Guid id, params Expression<Func<Product, object>>[] includes)
    {
        IQueryable<Product> query = _dbSet;
        foreach (var include in includes)
            query = query.Include(include);
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<Product>> GetAllAsync()
        => await _dbSet.Where(p => p.IsActive).ToListAsync();

    public virtual async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<Product> AddAsync(Product entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        return await _dbSet.CountAsync(predicate);
    }
}

/// <summary>
/// Category Repository Implementation
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    protected readonly ProductDbContext _context;
    protected readonly DbSet<Category> _dbSet;

    public CategoryRepository(ProductDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Category>();
    }

    public virtual async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<Category>> GetAllAsync()
        => await _dbSet.Where(c => c.IsActive).ToListAsync();

    public virtual async Task<Category> AddAsync(Category entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(Category entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
