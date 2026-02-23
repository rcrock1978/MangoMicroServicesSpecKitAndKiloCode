using Microsoft.EntityFrameworkCore;
using Mango.Services.EmailAPI.Domain;

namespace Mango.Services.EmailAPI.Infrastructure.Data;

/// <summary>
/// Email Database Context
/// </summary>
public class EmailDbContext : DbContext
{
    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options)
    {
    }

    public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;
    public DbSet<EmailLog> EmailLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EmailTemplate Configuration
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.ToTable("EmailTemplates");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            
            entity.Property(e => e.EmailType)
                .HasConversion<int>()
                .IsRequired();
            
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            entity.HasIndex(e => e.EmailType)
                .IsUnique();
        });

        // EmailLog Configuration
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.ToTable("EmailLogs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ToEmail)
                .IsRequired()
                .HasMaxLength(256);
            
            entity.Property(e => e.CcEmail)
                .HasMaxLength(256);
            
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Body)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
            
            entity.Property(e => e.EmailType)
                .HasConversion<int>()
                .IsRequired();
            
            entity.Property(e => e.Status)
                .HasConversion<int>()
                .IsRequired();
            
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);
            
            entity.HasIndex(e => e.ToEmail);
            entity.HasIndex(e => e.EmailType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SentAt);
        });
    }
}
