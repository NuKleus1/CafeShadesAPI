using Core.Entities;

namespace CafeShades.Models.Dtos
{
    public class MenuDto : ApiResponse
    {
        public List<CategoryDto> CategoryList { get; set; }

        public MenuDto(List<CategoryDto> categoryList) : base(true) 
        {
            CategoryList = categoryList;
        }
    }
}
