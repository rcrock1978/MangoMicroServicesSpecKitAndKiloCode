using Mango.Services.ProductAPI.Domain;

namespace Mango.Services.ProductAPI.Domain.Interfaces;

/// <summary>
/// Product Repository Interface
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByIdAsync(Guid id, params System.Linq.Expressions.Expression<Func<Product, object>>[] includes);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> FindAsync(System.Linq.Expressions.Expression<Func<Product, bool>> predicate);
    Task<Product> AddAsync(Product entity);
    Task UpdateAsync(Product entity);
    Task DeleteAsync(Guid id);
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<Product, bool>>? predicate = null);
}

/// <summary>
/// Category Repository Interface
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category> AddAsync(Category entity);
    Task UpdateAsync(Category entity);
    Task DeleteAsync(Guid id);
}
