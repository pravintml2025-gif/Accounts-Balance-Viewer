using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Adra.Core.Entities;
using Adra.Infrastructure.Identity;

namespace Adra.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<BalanceHistory> BalanceHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        });

        // BalanceHistory configuration
        modelBuilder.Entity<BalanceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.Year, e.Month }).IsUnique();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Account)
                .WithMany(a => a.BalanceHistories)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note: UploadedBy references ApplicationUser.Id but no navigation property
            entity.Property(e => e.UploadedBy).IsRequired();
        });

        // Seed data will be handled through a separate seeding service
        // SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Accounts with static GUIDs
        modelBuilder.Entity<Account>().HasData(
            new Account { Id = new Guid("11111111-1111-1111-1111-111111111111"), Name = "Cash", IsActive = true },
            new Account { Id = new Guid("22222222-2222-2222-2222-222222222222"), Name = "Petty Cash", IsActive = true },
            new Account { Id = new Guid("33333333-3333-3333-3333-333333333333"), Name = "CEO's car expenses", IsActive = true },
            new Account { Id = new Guid("44444444-4444-4444-4444-444444444444"), Name = "Stationery", IsActive = true },
            new Account { Id = new Guid("55555555-5555-5555-5555-555555555555"), Name = "Rent", IsActive = true }
        );
    }
}
