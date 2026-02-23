using MediatR;
using Mango.Services.RewardAPI.Application.DTOs;

namespace Mango.Services.RewardAPI.Application.Commands;

/// <summary>
/// Earn Points Command
/// </summary>
public record EarnPointsCommand(
    string UserId,
    int Points,
    string Description,
    string? OrderId
) : IRequest<UserRewardDto>;

/// <summary>
/// Redeem Points Command
/// </summary>
public record RedeemPointsCommand(
    string UserId,
    int Points,
    string Description,
    Guid? RewardId
) : IRequest<UserRewardDto?>;

/// <summary>
/// Create Reward Command
/// </summary>
public record CreateRewardCommand(
    string Name,
    string Description,
    int PointsRequired,
    string? ImageUrl,
    int MaxAvailable
) : IRequest<RewardDto>;

/// <summary>
/// Delete Reward Command
/// </summary>
public record DeleteRewardCommand(Guid Id) : IRequest<bool>;
