using Microsoft.EntityFrameworkCore;
using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Infrastructure.Data;

/// <summary>
/// Coupon Database Context
/// </summary>
public class CouponDbContext : DbContext
{
    public CouponDbContext(DbContextOptions<CouponDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Coupon configuration
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
    }
}
