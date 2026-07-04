using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TrackedProduct> TrackedProducts => Set<TrackedProduct>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ViewedProduct> ViewedProducts => Set<ViewedProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrackedProduct>(entity =>
        {
            entity.HasIndex(t => t.Query).IsUnique();
        });

        modelBuilder.Entity<PriceSnapshot>(entity =>
        {
            entity.HasIndex(s => new { s.TrackedProductId, s.CheckedAt });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.TrackedProductId, f.StoreName }).IsUnique();
        });

        modelBuilder.Entity<ViewedProduct>(entity =>
        {
            entity.HasIndex(v => new { v.UserId, v.TrackedProductId }).IsUnique();
        });
    }
}
