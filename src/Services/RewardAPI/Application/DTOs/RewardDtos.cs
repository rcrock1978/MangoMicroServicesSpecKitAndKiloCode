using Mango.Services.RewardAPI.Domain;

namespace Mango.Services.RewardAPI.Application.DTOs;

/// <summary>
/// User Reward DTO
/// </summary>
public class UserRewardDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public int LifetimePoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RewardTransactionDto> Transactions { get; set; } = new();
}

/// <summary>
/// Reward Transaction DTO
/// </summary>
public class RewardTransactionDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Reward DTO
/// </summary>
public class RewardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsRequired { get; set; }
    public string? ImageUrl { get; set; }
    public int MaxAvailable { get; set; }
    public int RedeemedCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTOs
/// </summary>
public class EarnPointsRequest
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OrderId { get; set; }
}

public class RedeemPointsRequest
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? RewardId { get; set; }
}

public class CreateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsRequired { get; set; }
    public string? ImageUrl { get; set; }
    public int MaxAvailable { get; set; }
}
