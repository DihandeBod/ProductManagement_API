using Microsoft.EntityFrameworkCore;
using ProductManagement_MOYO.Models;

namespace ProductManagement_MOYO.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(p =>
            {
                p.HasOne(pc => pc.ProductCategory)
                .WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pt => pt.ProductType)
                .WithMany(pc => pc.ProductCategories)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductLake> Lake { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
    }
}
