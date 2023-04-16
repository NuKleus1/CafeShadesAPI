using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Category>()
            //    .HasMany(cat => cat.Products)
            //    .WithOne(pro => pro.Category)
            //    .HasForeignKey(pro => pro.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(pro => pro.Category).WithMany(cat => cat.Products)
                .HasForeignKey(pro => pro.CategoryId);

            modelBuilder.Entity<Order>()
            .HasOne<User>(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasMany<OrderItem>(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne<OrderStatus>(o => o.OrderStatus)
                .WithMany()
                .HasForeignKey(oi => oi.OrderStatusId);
        }
    }
}
