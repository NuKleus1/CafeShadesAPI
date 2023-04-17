#nullable disable
namespace Cafeshades.Models.Request
{
    public class CategoryRequest
    {
        public string CategoryName {  get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
