using Microsoft.EntityFrameworkCore;
using PriceFinderAI.Core.Models;

namespace PriceFinderAI.Infrastructure.Data;

public sealed class PriceHistoryDbContext(DbContextOptions<PriceHistoryDbContext> options) : DbContext(options)
{
    public DbSet<TrackedProduct> TrackedProducts => Set<TrackedProduct>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

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
    }
}
