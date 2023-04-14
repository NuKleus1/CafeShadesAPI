#nullable disable

namespace CafeShades.Models.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public List<ProductDto> ProductList { get; set; }

    }
}