#nullable disable
namespace Core.Entities
{
    public class Order : BaseEntity
    {
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public int TotalAmount { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
