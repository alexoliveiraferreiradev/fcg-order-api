using Fcg.Orders.Application.IntegrationTests.Fixtures;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fcg.Orders.Application.IntegrationTests.Infrastructure
{
    [Collection("Mongo")]
    public class CustomerRepositoryTests
    {
        private readonly MongoFixture _fixture;

        public CustomerRepositoryTests(MongoFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task AddCustomer_GetCustomerByIdAsync_RoundTripPreservesAllFields()
        {
            var db = _fixture.CreateDatabase();
            var repo = new CustomerRepository(db);
            var id = Guid.NewGuid();

            var returned = await repo.AddCustomer(new CustomerSnapshot(id, "Maria", "maria@fcg.com"));
            var loaded = await repo.GetCustomerByIdAsync(id, CancellationToken.None);

            returned.Should().Be(id);
            loaded.CustomerId.Should().Be(id);
            loaded.Name.Should().Be("Maria");
            loaded.Email.Should().Be("maria@fcg.com");

            var raw = await db.GetCollection<BsonDocument>("customers").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
            raw["_id"].BsonType.Should().Be(BsonType.String);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_UnknownId_ReturnsNull()
        {
            var repo = new CustomerRepository(_fixture.CreateDatabase());

            (await repo.GetCustomerByIdAsync(Guid.NewGuid(), CancellationToken.None)).Should().BeNull();
        }

        [Fact]
        public async Task AddCustomer_DuplicateId_ThrowsMongoWriteException()
        {
            var repo = new CustomerRepository(_fixture.CreateDatabase());
            var id = Guid.NewGuid();
            await repo.AddCustomer(new CustomerSnapshot(id, "Maria", "maria@fcg.com"));

            var act = async () => await repo.AddCustomer(new CustomerSnapshot(id, "Outra", "outra@fcg.com"));

            await act.Should().ThrowAsync<MongoWriteException>();
        }
    }
}
