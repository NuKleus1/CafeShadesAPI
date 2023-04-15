using Core.Entities;
using System.Text.Json;

namespace Infrastructure.Data
{
    public class StoreDbSeed
    {
        public static async Task SeedAsync(StoreDbContext context)
        {

            if (!context.User.Any())
            {
                var data =
                    File.ReadAllText("../Infrastructure/Data/SeedData/User.json");

                var users = JsonSerializer.Deserialize<List<User>>(data);

                foreach (var item in users)
                    context.User.Add(item);
                context.SaveChanges();
            }


            if (!context.Category.Any())
            {
                var data =
                    File.ReadAllText("../Infrastructure/Data/SeedData/Categories.json");

                var categories = JsonSerializer.Deserialize<List<Category>>(data);

                foreach (var item in categories)
                    context.Category.Add(item);
                context.SaveChanges();

            }

            if (!context.Product.Any())
            {
                var data =
                    File.ReadAllText("../Infrastructure/Data/SeedData/Products.json");

                var products = JsonSerializer.Deserialize<List<Product>>(data);

                foreach (var item in products)
                    context.Product.Add(item);
                context.SaveChanges();

            }

            if (!context.OrderStatus.Any())
            {
                var data =
                    File.ReadAllText("../Infrastructure/Data/SeedData/OrderStatus.json");

                var status = JsonSerializer.Deserialize<List<OrderStatus>>(data);

                foreach (var item in status)
                    context.OrderStatus.Add(item);
                context.SaveChanges();

            }
            if (!context.Order.Any())
            {
                var data =
                    File.ReadAllText("../Infrastructure/Data/SeedData/Order.json");

                var orders = JsonSerializer.Deserialize<List<Order>>(data);

                foreach (var item in orders)
                    context.Order.Add(item);
                context.SaveChanges();

            }


            await context.SaveChangesAsync();
        }
    }
}

