namespace Mango.Services.RewardAPI.Domain;

/// <summary>
/// User Reward Points entity
/// </summary>
public class UserReward
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int AvailablePoints { get; set; }
    public int LifetimePoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<RewardTransaction> Transactions { get; set; } = new List<RewardTransaction>();
}

/// <summary>
/// Reward Transaction entity
/// </summary>
public class RewardTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserRewardId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public UserReward? UserReward { get; set; }
}

/// <summary>
/// Reward entity
/// </summary>
public class Reward
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsRequired { get; set; }
    public string? ImageUrl { get; set; }
    public int MaxAvailable { get; set; }
    public int RedeemedCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Transaction Type enumeration
/// </summary>
public enum TransactionType
{
    Earned = 0,
    Redeemed = 1,
    Expired = 2,
    Adjusted = 3
}
