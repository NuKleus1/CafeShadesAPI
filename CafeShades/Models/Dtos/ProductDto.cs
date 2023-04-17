#nullable disable
namespace Cafeshades.Models.Dtos
{
    public class ProductDto
    {
        public int productId { get; set; }
        public string productName { get; set; }
        public string productImage{ get; set; }
        public int productPrice{ get; set; }
        public string productCategory{ get; set; }
        public int productCategoryId{ get; set; }
    }
}
