namespace Fcg.Orders.Application.Features.Response
{
    public class OrderItemResponse
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public decimal PaidPrice { get; set; }
        public OrderItemResponse()
        {

        }
    }
}
