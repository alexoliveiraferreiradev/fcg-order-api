using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Interfaces
{
    public interface IOrderQueryRepository
    {
        Task<IEnumerable<Order>> GetOrderHistoryByUserId(Guid userId);
    }
}
