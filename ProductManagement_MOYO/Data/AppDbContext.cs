using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ProductManagement_MOYO.Models;
using System.Collections.Generic;

namespace ProductManagement_MOYO.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User - Role
            modelBuilder.Entity<UserAccount>()
                .HasOne(r => r.Role)
                .WithMany(u => u.UserAccounts)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Vendor
            modelBuilder.Entity<Vendor>()
                .HasOne(u => u.UserAccount)
                .WithOne(v => v.Vendor)
                .HasForeignKey<Vendor>(id => id.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product - ProductCategory
            modelBuilder.Entity<Product>(p =>
            {
                p.HasOne(pc => pc.ProductCategory)
                .WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductCategory - ProductType
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pt => pt.ProductType)
                .WithMany(pc => pc.ProductCategories)
                .OnDelete(DeleteBehavior.Restrict);

            // VendorProduct - Product & Vendor
            modelBuilder.Entity<VendorProduct>(vp =>
            {
                vp.HasOne(p => p.Product)
                .WithMany(v => v.VendorProducts)
                .OnDelete(DeleteBehavior.NoAction);

                vp.HasOne(v => v.Vendor)
                .WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.NoAction);
            });

            // OrderLine - Product
            modelBuilder.Entity<OrderLine>(ol =>
            {
                ol.HasOne(p => p.Product)
                .WithMany(line => line.OrderLines)
                .OnDelete(DeleteBehavior.Restrict);

                ol.HasOne(o => o.Order)
                .WithMany(line => line.Lines)
                .OnDelete(DeleteBehavior.Restrict);
            });


            // Order - Order Status & User & Vendor
            modelBuilder.Entity<Order>(o =>
            {
                o.HasOne(os => os.OrderStatus)
                .WithMany(s => s.Orders)
                .OnDelete(DeleteBehavior.Restrict);

                o.HasOne(u => u.User)
                .WithMany(or => or.Orders)
                .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(v => v.Vendor)
                .WithMany(or => or.Orders)
                .OnDelete(DeleteBehavior.NoAction);
            });

            // Seed data

            // For roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Product Manager" },
                new Role { RoleId = 2, RoleName = "Product Capturer" },
                new Role { RoleId = 3, RoleName = "Vendor" },
                new Role { RoleId = 4, RoleName = "Customer" }
            );

            // For OrderStatuses
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { OrderStatusId = 1, StatusName = "Unallocated"},
                new OrderStatus { OrderStatusId = 2, StatusName = "Order Placed" },
                new OrderStatus { OrderStatusId = 3, StatusName = "Order Packed" },
                new OrderStatus { OrderStatusId = 4, StatusName = "On the way" },
                new OrderStatus { OrderStatusId = 5, StatusName = "Delivered" }
            );

            // For ProductType
            modelBuilder.Entity<ProductType>().HasData(
                new ProductType { ProductTypeId = 1, ProductTypeName = "Devices" },
                new ProductType { ProductTypeId = 2, ProductTypeName = "Wearables" }
            );

            // For ProductCategory
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { ProductCategoryId = 1, ProductCategoryName = "Phones", ProductTypeId = 1 },
                new ProductCategory { ProductCategoryId = 2, ProductCategoryName = "Laptops", ProductTypeId = 1 },
                new ProductCategory { ProductCategoryId = 3, ProductCategoryName = "Digital Watches", ProductTypeId = 2 },
                new ProductCategory { ProductCategoryId = 4, ProductCategoryName = "Mechanical Watches", ProductTypeId = 2 }
            );

            // For Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "iPhone 15", ProductDescription = "The all new iPhone 15", IsDeleted = false, IsApproved = true, ProductCategoryId = 1 },
                new Product { ProductId = 2, ProductName = "Lenovo IdeaPad 3", ProductDescription = "Very good laptop", IsDeleted = false, IsApproved = true, ProductCategoryId = 2 },
                new Product { ProductId = 3, ProductName = "Apple Watch 9", ProductDescription = "The all new Apple Watch", IsDeleted = false, IsApproved = true, ProductCategoryId = 3 },
                new Product { ProductId = 4, ProductName = "Casio", ProductDescription = "The old fashioned Casio", IsDeleted = false, IsApproved = true, ProductCategoryId = 4 }
            );

            // For Users
            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount { Id = 1, GitHubId = null, Username = "Dihandb", Email = "dihandb@icloud.com", PasswordHash = "$2a$11$xXKOpnOEuWOixdZqB0SHruQWRYOrPvDtiIMnjYFVcNbrtK3.SFcAK", OAuthProvider = "Default", OAuthId = null, Name = null, RoleId = 3 },
                new UserAccount { Id = 2, GitHubId = null, Username = "u21451193", Email = "u21451193@tuks.co.za", PasswordHash = "$2a$11$Zmogmt/qZzJ26o2bCSGM1.SbBmJeMa1WQIfpo9eEHUkfpc96xTzVW", OAuthProvider = "Default", OAuthId = null, Name = null, RoleId = 3 }
            );

            // For Vendors
            modelBuilder.Entity<Vendor>().HasData(
                new Vendor { VendorId = 1, UserId = 1, VendorName = "Apple" },
                new Vendor { VendorId = 2, UserId = 2, VendorName = "Samsung" }
            );

            // For VendorProduct
            modelBuilder.Entity<VendorProduct>().HasData(
                new VendorProduct { VendorProductId = 1, VendorId = 1, ProductId = 1, Price = 100, QuantityOnHand = 10, StockLimit = 5, isActive = true },
                new VendorProduct { VendorProductId = 2, VendorId = 1, ProductId = 2, Price = 200, QuantityOnHand = 10, StockLimit = 5, isActive = true },
                new VendorProduct { VendorProductId = 3, VendorId = 1, ProductId = 3, Price = 150, QuantityOnHand = 10, StockLimit = 5, isActive = true },
                new VendorProduct { VendorProductId = 4, VendorId = 1, ProductId = 4, Price = 400, QuantityOnHand = 10, StockLimit = 5, isActive = true },
                new VendorProduct { VendorProductId = 5, VendorId = 2, ProductId = 1, Price = 200, QuantityOnHand = 10, StockLimit = 5, isActive = true },
                new VendorProduct { VendorProductId = 6, VendorId = 2, ProductId = 3, Price = 300, QuantityOnHand = 10, StockLimit = 5, isActive = true }
            );

           
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }

        public DbSet<ProductLake> Lake { get; set; }
        
        public DbSet<UserAccount> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorProduct> VendorProducts { get; set; }
        
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
    }
}
