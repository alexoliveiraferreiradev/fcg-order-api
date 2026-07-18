using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Interfaces
{
    public interface IOrderRepository
    {
        void AddOrderAsync(Order order);
    }
}
