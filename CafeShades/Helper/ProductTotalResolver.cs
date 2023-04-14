using AutoMapper;
using AutoMapper.Execution;
using CafeShades.Models.Dtos;
using Core.Entities;

namespace CafeShades.Helper
{
    public class ProductTotalResolver : IValueResolver<Product, ProductDto, int>
    {
        public int Resolve(Product src, ProductDto destination, int destMember, ResolutionContext context)
        {
            return src.Price * src.Quanity;
        }
    }
}
