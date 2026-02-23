using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Domain;

namespace Mango.Services.RewardAPI.Infrastructure.Data;

/// <summary>
/// Reward Database Context
/// </summary>
public class RewardDbContext : DbContext
{
    public RewardDbContext(DbContextOptions<RewardDbContext> options) 
        : base(options)
    {
    }

    public DbSet<UserReward> UserRewards => Set<UserReward>();
    public DbSet<RewardTransaction> RewardTransactions => Set<RewardTransaction>();
    public DbSet<Reward> Rewards => Set<Reward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserReward configuration
        modelBuilder.Entity<UserReward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // RewardTransaction configuration
        modelBuilder.Entity<RewardTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasMaxLength(100);
            
            entity.HasOne(e => e.UserReward)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserRewardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Reward configuration
        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
