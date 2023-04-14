#nullable disable

namespace CafeShades.Models.Dtos
{
    public class OrderDto
    {
        
        public int orderId { get; set; }
        
        public string orderStatus { get; set; }
        
        public DateTime orderDate { get; set; }
        
        public int totalAmount { get; set; }
        
        public List<ProductDto> productList { get; set; }

       
    }
}
