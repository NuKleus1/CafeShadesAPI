#nullable disable
namespace CafeShades.Models.Request
{
    public class ProductRequest
    {
        public string productName { get; set; }
        public IFormFile productImage { get; set; }
        public int productPrice { get; set; }
        public int productCategoryId { get; set; }
    }
}
