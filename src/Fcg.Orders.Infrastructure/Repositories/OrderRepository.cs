using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Repositories;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Order> _collection;

        public OrderRepository( IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<Order>("orders"); 
        }

        public async Task<Guid> AddOrder(Order order)
        {
            await _collection.InsertOneAsync(order);
            return order.Id;
        }

        public async Task<bool> CustomerOwnsGameAsync(Guid customerId, Guid gameId,CancellationToken cancellation)
        {
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.Customer.CustomerId, customerId),
                Builders<Order>.Filter.Eq("Games.GameId", gameId.ToString()),
                Builders<Order>.Filter.Eq(o=>o.Status, OrderStatus.Completed)
            );
           
            return await _collection.Find(filter).AnyAsync(cancellation);
        }

        public async Task<Order> GetOrderById(Guid orderId,CancellationToken cancellationToken)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, orderId);
            return await _collection.Find(filter).SingleOrDefaultAsync(cancellationToken);  
        }

        public async Task<bool> UpdateOrder(Order order)
        {
            var result = await _collection.ReplaceOneAsync(x => x.Id == order.Id, order);
            return result.ModifiedCount > 0;
        }
    }
}
