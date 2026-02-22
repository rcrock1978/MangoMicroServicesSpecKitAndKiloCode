using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mango.Services.AuthAPI.Application.DTOs;
using Mango.Services.AuthAPI.Domain.Entities;
using Mango.Services.AuthAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.AuthAPI.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash, user.Salt))
        {
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive");
        }

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, token, refreshToken);

        _logger.LogInformation("User logged in successfully {Email}", request.Email);

        return new AuthResponseDto(user.Id, user.Email, user.Name, token, refreshToken);
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            Role = "Customer"
        };

        // Generate salt and hash password together
        user.Salt = GenerateSalt();
        user.PasswordHash = HashPassword(request.Password, user.Salt);

        await _userRepository.AddAsync(user);

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, token, refreshToken);

        _logger.LogInformation("User registered successfully {Email}", request.Email);

        return new AuthResponseDto(user.Id, user.Email, user.Name, token, refreshToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.IsUsed || refreshToken.IsRevoked)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (refreshToken.ExpiredAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Mark old token as used
        refreshToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        // Generate new tokens
        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, newToken, newRefreshToken);

        _logger.LogInformation("Token refreshed for user {Email}", user.Email);

        return new AuthResponseDto(user.Id, user.Email, user.Name, newToken, newRefreshToken);
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenExpiryMinutes = double.TryParse(_configuration["Jwt:TokenExpiryMinutes"], out var expiry) 
            ? expiry : 15;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task SaveRefreshTokenAsync(Guid userId, string jwtId, string refreshToken)
    {
        var refreshTokenExpiryDays = double.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var expiry) 
            ? expiry : 7;

        var token = new UserRefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            JwtId = jwtId,
            IssuedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
        };

        await _refreshTokenRepository.AddAsync(token);
    }

    private static string GenerateSalt()
    {
        var salt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    private static string HashPassword(string password, string salt)
    {
        // If salt is empty, generate a new one (for new user registration)
        if (string.IsNullOrEmpty(salt))
        {
            salt = GenerateSalt();
        }
        
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            Encoding.UTF8.GetBytes(salt),
            100000,
            HashAlgorithmName.SHA256);
        var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        return hash + "." + salt;
    }

    private static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
            return false;
        
        var parts = storedHash.Split('.');
        if (parts.Length != 2)
            return false;
            
        var hash = HashPassword(password, storedSalt);
        return hash == storedHash;
    }
}
