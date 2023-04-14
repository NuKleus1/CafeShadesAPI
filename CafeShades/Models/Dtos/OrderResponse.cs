namespace CafeShades.Models.Dtos
{
    public class OrderResponse :ApiResponse
    {
        public List<OrderDto> OrderList { get; set; }

        public OrderResponse(List<OrderDto> orderList) : base(true)
        {
            OrderList = orderList;
        }
    }
}
