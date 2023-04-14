using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
namespace Infrastructure.Data.Repository
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly DbSet<Category> _categories;
        public CategoryRepository(StoreDbContext context) : base(context)
        {
            _categories = context.Set<Category>();
        }

        public async Task<List<Category>> GetAllWithProducts()
        {
            return await _categories.Include(cat => cat.Products).ToListAsync();
        }
    }
}
