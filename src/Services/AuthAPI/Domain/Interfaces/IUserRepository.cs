using Mango.Services.AuthAPI.Domain.Entities;

namespace Mango.Services.AuthAPI.Domain.Interfaces;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);
}

/// <summary>
/// Refresh token repository interface
/// </summary>
public interface IRefreshTokenRepository
{
    Task<UserRefreshToken?> GetByTokenAsync(string token);
    Task<UserRefreshToken> AddAsync(UserRefreshToken token);
    Task UpdateAsync(UserRefreshToken token);
    Task<UserRefreshToken?> GetByUserIdAndJwtIdAsync(Guid userId, string jwtId);
}
