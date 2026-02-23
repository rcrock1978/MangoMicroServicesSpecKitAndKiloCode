using Mango.Services.RewardAPI.Domain;

namespace Mango.Services.RewardAPI.Domain.Interfaces;

/// <summary>
/// User Reward Repository Interface
/// </summary>
public interface IUserRewardRepository
{
    Task<UserReward?> GetByUserIdAsync(string userId);
    Task<UserReward?> GetByUserIdWithTransactionsAsync(string userId);
    Task<UserReward> AddAsync(UserReward entity);
    Task UpdateAsync(UserReward entity);
}

/// <summary>
/// Reward Repository Interface
/// </summary>
public interface IRewardRepository
{
    Task<Reward?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reward>> GetAllAsync();
    Task<Reward> AddAsync(Reward entity);
    Task UpdateAsync(Reward entity);
    Task DeleteAsync(Guid id);
}
