using Mango.Services.OrderAPI.Domain;

namespace Mango.Services.OrderAPI.Domain.Interfaces;

/// <summary>
/// Order Repository Interface
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithItemsAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Order>> GetAllAsync(int page, int pageSize);
    Task<Order> AddAsync(Order entity);
    Task UpdateAsync(Order entity);
    Task<int> CountAsync();
}
