using Fcg.Orders.Domain.Enum;

namespace Fcg.Orders.Application.Features.Response
{
    public class OrderHistoryResponse
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public OrderHistoryResponse()
        {

        }
    }
}
