namespace CafeShades.Models.Dtos
{
    public class OrderItemDto
    {
        public string productName { get; set; }
        public int productId { get; set; }
        public int productQuantity { get; set; }
        public int productPrice { get; set; }
    }
}
