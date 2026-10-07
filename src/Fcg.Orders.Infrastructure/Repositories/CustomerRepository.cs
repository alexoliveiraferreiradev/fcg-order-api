using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Repositories;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerSnapshotRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<CustomerSnapshot> _collection;

        public CustomerRepository(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<CustomerSnapshot>("customers");
        }

        public async Task<Guid> AddCustomer(CustomerSnapshot customer)
        {
            await _collection.InsertOneAsync(customer);
            return customer.CustomerId;        
        }

        public async Task<CustomerSnapshot> GetCustomerByIdAsync(Guid id, CancellationToken cancellation)
        {
            var filter = Builders<CustomerSnapshot>.Filter.Eq(c => c.CustomerId, id);
            return await _collection.Find(filter).SingleOrDefaultAsync(cancellation);
        }
    }
}
