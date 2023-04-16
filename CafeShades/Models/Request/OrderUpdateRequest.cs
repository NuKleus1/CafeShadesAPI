using Core.Entities;

namespace CafeShades.Models.Request
{
    public class OrderUpdateRequest
    {
        public int UserId { get; set; }
        public int OrderStatusId { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
