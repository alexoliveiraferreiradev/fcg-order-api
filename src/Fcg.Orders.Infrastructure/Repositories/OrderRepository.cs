using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Repositories;
using Fcg.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddOrder(Order order)
        {
            _dbContext.Orders.Add(order);   
        }

        public async Task<Order> GetOrderById(Guid orderId)
        {
            return await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
        }

        public void UpdateOrder(Order order)
        {
            _dbContext.Orders.Update(order);    
        }
    }
}
