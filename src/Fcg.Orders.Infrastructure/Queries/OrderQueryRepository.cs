using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Domain.Entities;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Queries
{
    public class OrderQueryRepository : IOrderQueryRepository
    {
        public const int MaxPageSize = 100;
        private readonly IMongoDatabase _database;

        public OrderQueryRepository(IMongoDatabase database)
        {
            _database = database;   
        }

        public async Task<PagedResult<Order>> GetOrderHistoryByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
            page = Math.Max(page, 1);

            var collection = _database.GetCollection<Order>("orders");
            var filter = Builders<Order>.Filter.Eq(o => o.Customer.CustomerId, userId);
            
            var totalCount = (Int32)await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            
            var sort = Builders<Order>.Sort
                .Descending(o => o.CreatedAt)
                .Descending(o => o.Status);

            var items = await collection
                .Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Order>(items, page, pageSize, totalCount);

        }
        
    }
}
