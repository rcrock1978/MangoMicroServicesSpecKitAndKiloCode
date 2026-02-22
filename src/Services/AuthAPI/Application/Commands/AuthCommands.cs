using MediatR;
using Mango.Services.AuthAPI.Application.DTOs;

namespace Mango.Services.AuthAPI.Application.Commands;

/// <summary>
/// Login command
/// </summary>
public record LoginCommand(LoginRequestDto Request) : IRequest<AuthResponseDto>;

/// <summary>
/// Register command
/// </summary>
public record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthResponseDto>;

/// <summary>
/// Refresh token command
/// </summary>
public record RefreshTokenCommand(RefreshTokenRequestDto Request) : IRequest<AuthResponseDto>;
