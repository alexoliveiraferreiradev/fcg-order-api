using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Repositories
{
    public interface IOrderRepository
    {
        void AddOrder(Order order);
        Task<Order> GetOrderById(Guid orderId);
        void UpdateOrder(Order order); 
    }
}
