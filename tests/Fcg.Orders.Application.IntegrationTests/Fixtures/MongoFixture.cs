using Fcg.Orders.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Fcg.Orders.Application.IntegrationTests.Fixtures
{
    // Um unico container MongoDB compartilhado por todos os testes da collection "Mongo".
    public sealed class MongoFixture : IAsyncLifetime
    {
        private readonly MongoDbContainer _container = new MongoDbBuilder().Build();
        private MongoClient? _client;

        public string ConnectionString => _container.GetConnectionString();

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
            MongoConventions.EnsureRegistered();
            _client = new MongoClient(ConnectionString);
        }

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        // Banco unico por teste para isolar os dados.
        public IMongoDatabase CreateDatabase() =>
            _client!.GetDatabase($"fcg_orders_test_{Guid.NewGuid():N}");
    }

    [CollectionDefinition("Mongo")]
    public sealed class MongoCollection : ICollectionFixture<MongoFixture> { }
}
