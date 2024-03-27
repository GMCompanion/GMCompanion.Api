using GMCompanion.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GMCompanion.Api.Infrastucture;

public class MarketContext : DbContext
{
    public DbSet<Character> Characters { get; set; }
    public DbSet<Item> Items { get; set; }

    public MarketContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(
        c =>
        {
            c.ToTable("Character");
            c.HasMany(c => c.Items).WithMany(i => i.Characters);
        });

        modelBuilder.Entity<Item>(
        i => 
        { 
            i.ToTable("Item");
            i.HasMany(i => i.Characters).WithMany(c => c.Items);
        });

        base.OnModelCreating(modelBuilder);
    }
}