using HongarijePlanner.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<WishlistVote> WishlistVotes => Set<WishlistVote>();
    public DbSet<ItineraryItem> ItineraryItems => Set<ItineraryItem>();
    public DbSet<ItineraryVote> ItineraryVotes => Set<ItineraryVote>();
    public DbSet<PackingItem> PackingItems => Set<PackingItem>();
    public DbSet<SharedPackingItem> SharedPackingItems => Set<SharedPackingItem>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<InfoItem> InfoItems => Set<InfoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WishlistVote>()
            .HasKey(vote => new { vote.ItemId, vote.UserName });

        modelBuilder.Entity<WishlistVote>()
            .HasOne(vote => vote.Item)
            .WithMany(item => item.Votes)
            .HasForeignKey(vote => vote.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItineraryVote>()
            .HasKey(vote => new { vote.ItemId, vote.UserName });

        modelBuilder.Entity<ItineraryVote>()
            .HasOne(vote => vote.Item)
            .WithMany(item => item.Votes)
            .HasForeignKey(vote => vote.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WishlistItem>()
            .Property(item => item.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<PackingItem>()
            .Property(item => item.IsPacked)
            .HasDefaultValue(false);

        modelBuilder.Entity<PackingItem>()
            .Property(item => item.IsDefault)
            .HasDefaultValue(false);

        modelBuilder.Entity<SharedPackingItem>()
            .Property(item => item.IsPacked)
            .HasDefaultValue(false);

        modelBuilder.Entity<ShoppingItem>()
            .Property(item => item.IsBought)
            .HasDefaultValue(false);

        base.OnModelCreating(modelBuilder);
    }
}
