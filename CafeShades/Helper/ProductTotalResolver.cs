using AutoMapper;
using AutoMapper.Execution;
using Cafeshades.Models.Dtos;
using Core.Entities;

namespace Cafeshades.Helper
{
    public class ProductTotalResolver : IValueResolver<Product, ProductDto, int>
    {
        public int Resolve(Product src, ProductDto destination, int destMember, ResolutionContext context)
        {
            return src.Price * src.Quanity;
        }
    }
}
