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
        });

        modelBuilder.Entity<Item>(
        i => 
        { 
            i.ToTable("Item");
        });

        modelBuilder.Entity<InventoryItem>(i =>
        {
            i.ToTable("Inventory");
            i.HasOne(i => i.Character).WithMany(c => c.Inventory).HasForeignKey(i => i.CharacterId);
            i.HasOne(i => i.Item).WithMany(c => c.Inventories).HasForeignKey(i => i.ItemId);
        });

        base.OnModelCreating(modelBuilder);
    }
}