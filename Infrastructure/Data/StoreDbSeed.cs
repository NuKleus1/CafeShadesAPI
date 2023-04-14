using Core.Entities;
using System.Text.Json;

namespace Infrastructure.Data
{
    public class StoreDbSeed
    {
        public static async Task SeedAsync(StoreDbContext context)
        {
            if (!context.Category.Any())
            {
                var categoryData =
                    File.ReadAllText("../Infrastructure/Data/SeedData/Categories.json");

                var categories = JsonSerializer.Deserialize<List<Category>>(categoryData);

                foreach (var item in categories)
                    context.Category.Add(item);
            }

            if (!context.Product.Any())
            {
                var productData =
                    File.ReadAllText("../Infrastructure/Data/SeedData/Products.json");

                var products = JsonSerializer.Deserialize<List<Product>>(productData);

                foreach (var item in products)
                    context.Product.Add(item);
            }
            await context.SaveChangesAsync();

        }
    }
}

