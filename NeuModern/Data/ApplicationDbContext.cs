using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeuModern.Models;
using System.Reflection.Emit;

namespace NeuModern.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{



    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<OrderHeader> OrderHeaders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<WishList> WishLists { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<MultipleAddress> MultipleAddresses { get; set; }
    //public DbSet<SalesReport> SalesReports { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
        .IsUnique();


        modelBuilder.Entity<Category>().HasData(
           new Category { Id = 1, Name = "T-Shirt", DisplayOrder = 1, IsActive = true },
           new Category { Id = 2, Name = "Casual Shirts", DisplayOrder = 2, IsActive = true },
           new Category { Id = 3, Name = "Formal Shirts", DisplayOrder = 3, IsActive = false },
           new Category { Id = 4, Name = "Jackets", DisplayOrder = 4, IsActive = true }
        );


        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,

                Name = "Colourblocked T-Shirt",
                Description = "A dress can have sleeves, straps, or be held up with elastic around the chest, leaving the shoulders bare. Dresses also vary in color. The hemlines of dresses vary depending on modesty, weather, fashion or the personal taste of the wearer.",
                Size = "Medium",
                StockQuantity = 5,
                Brand = "Roadster",
                Discount = 63,
                OfferPrice = 799,
                Price = 1749,
                DateTime = DateTime.Now,
                CategoryId = 1
            },

            new Product
            {
                Id = 2,

                Name = "Oversized T-Shirt",
                Description = "A dress can have sleeves, straps, or be held up with elastic around the chest, leaving the shoulders bare. Dresses also vary in color. The hemlines of dresses vary depending on modesty, weather, fashion or the personal taste of the wearer.",
                Size = "Small",
                StockQuantity = 1,
                Brand = "Bullmer",
                Discount = 74,
                OfferPrice = 389,
                Price = 1449,
                DateTime = DateTime.Now,
                CategoryId = 1
            },
            new Product
            {
                Id = 3,

                Name = "Casual Shirt",
                Description = "A dress can have sleeves, straps, or be held up with elastic around the chest, leaving the shoulders bare. Dresses also vary in color. The hemlines of dresses vary depending on modesty, weather, fashion or the personal taste of the wearer.",
                Size = "Medium",
                StockQuantity = 3,
                Brand = "Powerlook",
                Discount = 45,
                OfferPrice = 1099,
                Price = 1999,
                DateTime = DateTime.Now,
                CategoryId = 2
            },
            new Product
            {
                Id = 4,

                Name = "Denim Jacket",
                Description = "A dress can have sleeves, straps, or be held up with elastic around the chest, leaving the shoulders bare. Dresses also vary in color. The hemlines of dresses vary depending on modesty, weather, fashion or the personal taste of the wearer.",
                Size = "Larger",
                StockQuantity = 4,
                Brand = "Ketch",
                Discount = 78,
                OfferPrice = 659,
                Price = 2999,
                DateTime = DateTime.Now,
                CategoryId = 4
            },
            new Product
            {
                Id = 5,
                Name = "Slim Fit Shirt",
                Description = "A dress can have sleeves, straps, or be held up with elastic around the chest, leaving the shoulders bare. Dresses also vary in color. The hemlines of dresses vary depending on modesty, weather, fashion or the personal taste of the wearer.",
                Size = "Small",
                StockQuantity = 4,
                Brand = "Roadster",
                Discount = 70,
                OfferPrice = 479,
                Price = 1500,
                DateTime = DateTime.Now,
                CategoryId = 3
            }

            );
    }
}
