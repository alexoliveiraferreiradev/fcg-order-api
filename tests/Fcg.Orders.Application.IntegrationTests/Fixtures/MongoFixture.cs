using Fcg.Orders.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Fcg.Orders.Application.IntegrationTests.Fixtures
{
    // Um unico container MongoDB compartilhado por todos os testes da collection "Mongo".
    // Sem Docker na maquina local os testes sao pulados; no CI (CI=true) a falha e propagada.
    public sealed class MongoFixture : IAsyncLifetime
    {
        private MongoDbContainer? _container;
        private MongoClient? _client;

        public string? UnavailableReason { get; private set; }

        public string ConnectionString => _container!.GetConnectionString();

        public async Task InitializeAsync()
        {
            try
            {
                _container = new MongoDbBuilder().Build();
                await _container.StartAsync();
            }
            catch (Exception ex) when (!IsRunningOnCi())
            {
                UnavailableReason = $"Container MongoDB nao subiu (Docker indisponivel?), testes Mongo ignorados: {ex.Message}";
                return;
            }

            MongoConventions.EnsureRegistered();
            _client = new MongoClient(ConnectionString);
        }

        public Task DisposeAsync() =>
            _container is null ? Task.CompletedTask : _container.DisposeAsync().AsTask();

        // Chamar no inicio de cada [SkippableFact]/[SkippableTheory] da collection.
        public void SkipIfUnavailable() => Skip.If(UnavailableReason is not null, UnavailableReason);

        // Banco unico por teste para isolar os dados.
        public IMongoDatabase CreateDatabase() =>
            _client!.GetDatabase($"fcg_orders_test_{Guid.NewGuid():N}");

        private static bool IsRunningOnCi() =>
            string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);
    }

    [CollectionDefinition("Mongo")]
    public sealed class MongoCollection : ICollectionFixture<MongoFixture> { }
}
