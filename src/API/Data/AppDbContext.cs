using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ensure Name is unique or required if needed
        modelBuilder.Entity<Tag>().Property(t => t.Name).IsRequired();
        modelBuilder.Entity<Tag>().Property(t => t.Percentage).HasPrecision(18, 6);
    }
}
