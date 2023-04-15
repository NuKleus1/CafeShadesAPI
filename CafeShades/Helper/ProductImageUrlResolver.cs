using AutoMapper;
using AutoMapper.Execution;
using Cafeshades.Models.Dtos;
using Core.Entities;

namespace Cafeshades.Helper
{
    public class ProductImageUrlResolver : IValueResolver<Product, ProductDto, string>
    {
        private readonly IConfiguration _config;

        public ProductImageUrlResolver(IConfiguration config)
        {
            _config = config;
        }

        public string Resolve(Product source, ProductDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ImageUrl))
            {
                return _config["ApiUrl"] + "Product/" + source.ImageUrl;
            }
            return null;
        }
    }
}
