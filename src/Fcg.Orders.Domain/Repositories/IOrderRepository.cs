using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Guid> AddOrder(Order order);
        Task<Order> GetOrderById(Guid orderId,CancellationToken cancellation);
        Task<bool> UpdateOrder(Order order);
        Task<bool> CustomerOwnsGameAsync(Guid customerId, Guid gameId,CancellationToken cancellation);
    }
}
