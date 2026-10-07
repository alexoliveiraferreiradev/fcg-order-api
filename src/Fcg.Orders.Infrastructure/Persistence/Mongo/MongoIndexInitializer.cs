using Fcg.Orders.Domain.Entities;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Persistence.Mongo
{
    internal static class MongoIndexInitializer
    {
        public static async Task EnsureIndexesAsync(IMongoDatabase database)
        {
            await EnsureOrderIndexesAsync(database);
        }
        public static async Task EnsureOrderIndexesAsync(IMongoDatabase database)
        {
            var collection = database.GetCollection<Order>("orders");
            var keys = Builders<Order>.IndexKeys;

            var indexModels = new List<CreateIndexModel<Order>>
            {
                 new(keys.Ascending("games.gameId"),
                     new CreateIndexOptions { Name = "idx_game_id" }),

                new(keys.Ascending("customer.customerId").Ascending("games.gameId"),
                    new CreateIndexOptions { Name = "idx_customer_game_ownership" }),

                new(keys.Ascending("customer.customerId").Descending(o => o.CreatedAt),
                    new CreateIndexOptions { Name = "idx_customer_history" })
            };

            await collection.Indexes.CreateManyAsync(indexModels);
        }

       
    }
}
