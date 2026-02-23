using MediatR;
using Mango.Services.RewardAPI.Application.DTOs;

namespace Mango.Services.RewardAPI.Application.Queries;

/// <summary>
/// Get User Reward By User ID Query
/// </summary>
public record GetUserRewardByUserIdQuery(string UserId) : IRequest<UserRewardDto?>;

/// <summary>
/// Get All Rewards Query
/// </summary>
public record GetRewardsQuery() : IRequest<List<RewardDto>>;

/// <summary>
/// Get Reward By ID Query
/// </summary>
public record GetRewardByIdQuery(Guid Id) : IRequest<RewardDto?>;
