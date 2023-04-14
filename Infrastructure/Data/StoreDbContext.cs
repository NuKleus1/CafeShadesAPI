using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Category>()
            //    .HasMany(cat => cat.Products)
            //    .WithOne(pro => pro.Category)
            //    .HasForeignKey(pro => pro.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(pro => pro.Category).WithMany(cat => cat.Products)
                .HasForeignKey(pro => pro.CategoryId);
        }
    }
}
