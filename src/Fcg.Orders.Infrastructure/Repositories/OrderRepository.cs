using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Interfaces;
using Fcg.Orders.Infrastructure.Persistence;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddOrderAsync(Order order)
        {
            _dbContext.Orders.Add(order);   
        }
    }
}
