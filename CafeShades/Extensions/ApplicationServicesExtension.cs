using Cafeshades.Helper;
using Core.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Data;
using Infrastructure.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Cafeshades.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(typeof(MappingProfiles));

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddDbContext<StoreDbContext>(option =>
            {
                option.UseSqlServer(config.GetConnectionString("StoreDb"));
            });

            try
            {
                var defaultApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("firebase-key.json")
                });
                Console.Write("Firebase Connected");
            }catch(Exception ex)
            {
                Console.Write(ex);
                Console.WriteLine(ex.StackTrace);
            }

            return services;
        }
    }
}
