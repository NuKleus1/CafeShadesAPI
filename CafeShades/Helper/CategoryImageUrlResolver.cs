using AutoMapper;
using AutoMapper.Execution;
using Cafeshades.Models.Dtos;
using Core.Entities;

namespace Cafeshades.Helper
{
    public class CategoryImageUrlResolver : IValueResolver<Category, CategoryDto, string>
    {
        private readonly IConfiguration _config;

        public CategoryImageUrlResolver(IConfiguration config)
        {
            _config = config;
        }

        public string Resolve(Category source, CategoryDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ImageUrl))
            {
                return _config["ApiUrl"] + "Category/" + source.ImageUrl;
            }
            return null;
        }
    }
}
