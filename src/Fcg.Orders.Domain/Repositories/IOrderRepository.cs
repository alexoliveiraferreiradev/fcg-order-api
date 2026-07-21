using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Repositories
{
    public interface IOrderRepository
    {
        void AddOrderAsync(Order order);
        Task<Order> GetOrderById(Guid orderId);
    }
}
