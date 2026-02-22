namespace Mango.Services.AuthAPI.Application.DTOs;

/// <summary>
/// Login request DTO
/// </summary>
public record LoginRequestDto(
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    string Email, 
    [System.ComponentModel.DataAnnotations.Required]
    string Password);

/// <summary>
/// Register request DTO
/// </summary>
public record RegisterRequestDto(
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    string Email, 
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(8)]
    string Password, 
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(2)]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    string Name, 
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Phone]
    string PhoneNumber);

/// <summary>
/// Authentication response DTO
/// </summary>
public record AuthResponseDto(Guid UserId, string Email, string Name, string Token, string RefreshToken);

/// <summary>
/// User DTO
/// </summary>
public record UserDto(Guid Id, string Email, string Name, string PhoneNumber, string Role, bool EmailConfirmed);

/// <summary>
/// Token DTO
/// </summary>
public record TokenDto(string Token, string RefreshToken);

/// <summary>
/// Refresh token request DTO
/// </summary>
public record RefreshTokenRequestDto(string Token, string RefreshToken);
