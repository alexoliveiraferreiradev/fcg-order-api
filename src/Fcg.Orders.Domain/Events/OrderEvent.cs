using MediatR;

namespace Fcg.Orders.Domain.Events
{
    public class OrderEvent : INotification
    {
        public Guid UserId { get; set; }
        public OrderEvent(Guid userId)
        {
            UserId = userId;    
        }
    }
}
