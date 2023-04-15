using Core.Entities;

namespace CafeShades.Models.Request
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}