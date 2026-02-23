using Mango.Services.ShoppingCartAPI.Domain;

namespace Mango.Services.ShoppingCartAPI.Domain.Interfaces;

/// <summary>
/// Cart Repository Interface
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId);
    Task<Cart?> GetByUserIdWithItemsAsync(string userId);
    Task<Cart> AddAsync(Cart entity);
    Task UpdateAsync(Cart entity);
    Task DeleteAsync(Guid id);
    Task DeleteByUserIdAsync(string userId);
}
