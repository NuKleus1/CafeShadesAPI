using AutoMapper;
using AutoMapper.Execution;
using Cafeshades.Models.Dtos;
using Core.Entities;

namespace Cafeshades.Helper
{
    public class ProductImageUrlFromOrderItem : IValueResolver<OrderItem, ProductDto, string>
    {
        private readonly IConfiguration _config;

        public ProductImageUrlFromOrderItem(IConfiguration config)
        {
            _config = config;
        }

        public string Resolve(OrderItem source, ProductDto destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return null;

            if (source.Product == null)
                return null;

            if (string.IsNullOrEmpty(source.Product.ImageUrl))
                return null;

            return _config["ApiUrl"] + "Product/" + source.Product.ImageUrl;

        }
    }
}
