using GestionProductos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionProductos.Api.Data;

// Acceso a la base de datos de SQL Server. Las tablas las crea database/script.sql;
// aquí solo se describe cómo se corresponden con las clases.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(50);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(100);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.PasswordHash).HasMaxLength(100);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.Property(t => t.Name).HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(p => p.ProductType).WithMany(t => t.Products).HasForeignKey(p => p.ProductTypeId);
        });
    }
}
