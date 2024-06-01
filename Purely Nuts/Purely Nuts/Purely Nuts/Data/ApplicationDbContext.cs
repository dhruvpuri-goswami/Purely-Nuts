using Microsoft.EntityFrameworkCore;
using Purely_Nuts.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Unique of username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        //Unique of email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.ProductName)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> User { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<Cart> Cart { get; set; }
}