using CafeShades.Models;
using Core.Entities;

namespace Cafeshades.Models.Dtos
{
    public class MenuDto : ApiResponse
    {
        public IReadOnlyList<CategoryDto> CategoryList { get; set; }

        public MenuDto(IReadOnlyList<CategoryDto> categoryList) : base(true) 
        {
            CategoryList = categoryList;
        }
    }
}
