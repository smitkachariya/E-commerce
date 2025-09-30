using E_commerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductImage> ProductImages { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<CartItem> CartItems { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure decimal precision for prices
			modelBuilder.Entity<Product>()
				.Property(p => p.Price)
				.HasColumnType("decimal(18,2)");

			modelBuilder.Entity<OrderItem>()
				.Property(o => o.Price)
				.HasColumnType("decimal(18,2)");

			// Configure relationships
			modelBuilder.Entity<Product>()
				.HasOne(p => p.Seller)
				.WithMany()
				.HasForeignKey(p => p.SellerId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ProductImage>()
				.HasOne(pi => pi.Product)
				.WithMany(p => p.Images)
				.HasForeignKey(pi => pi.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure CartItem relationships
			modelBuilder.Entity<CartItem>()
				.HasOne(ci => ci.User)
				.WithMany()
				.HasForeignKey(ci => ci.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<CartItem>()
				.HasOne(ci => ci.Product)
				.WithMany()
				.HasForeignKey(ci => ci.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// Seed categories (optional)
			modelBuilder.Entity<Product>().HasData();
		}
	}
}


