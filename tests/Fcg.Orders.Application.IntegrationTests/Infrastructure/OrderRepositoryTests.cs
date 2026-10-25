using Fcg.Orders.Application.IntegrationTests.Fixtures;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fcg.Orders.Application.IntegrationTests.Infrastructure
{
    [Collection("Mongo")]
    public class OrderRepositoryTests
    {
        private readonly MongoFixture _fixture;

        public OrderRepositoryTests(MongoFixture fixture) => _fixture = fixture;

        private static Order NewOrder(Guid? customerId = null, params (Guid id, string name, decimal price)[] games)
        {
            var order = new Order(new OrderCustomer(customerId ?? Guid.NewGuid(), "Cliente", "cliente@fcg.com"));
            foreach (var g in games) order.AddItem(g.id, g.name, g.price);
            order.MakeOrder();
            return order;
        }

        [SkippableFact]
        public async Task AddOrder_GetOrderById_RoundTripPreservesAllFields()
        {
            _fixture.SkipIfUnavailable();

            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var g1 = Guid.NewGuid(); var g2 = Guid.NewGuid();
            var order = NewOrder(null, (g1, "Jogo A", 10.55m), (g2, "Jogo B", 20.45m));

            var id = await repo.AddOrder(order);
            var loaded = await repo.GetOrderById(id, CancellationToken.None);

            loaded.Should().NotBeNull();
            loaded.Id.Should().Be(order.Id);
            loaded.Status.Should().Be(OrderStatus.Draft);
            loaded.Customer.CustomerId.Should().Be(order.Customer.CustomerId);
            loaded.Customer.Name.Should().Be("Cliente");
            loaded.Customer.Email.Should().Be("cliente@fcg.com");
            loaded.TotalAmount.Amount.Should().Be(31.00m);
            loaded.Games.Should().HaveCount(2);
            loaded.Games.Should().ContainSingle(g => g.GameId == g1 && g.GameName == "Jogo A" && g.GameAmount == 10.55m);
            loaded.Games.Should().ContainSingle(g => g.GameId == g2 && g.GameName == "Jogo B" && g.GameAmount == 20.45m);
            loaded.CreatedAt.Should().BeCloseTo(order.CreatedAt, TimeSpan.FromMilliseconds(1));
            loaded.UpdatedAt.Should().BeCloseTo(order.UpdatedAt, TimeSpan.FromMilliseconds(1));
            loaded.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
            loaded.UpdatedAt.Kind.Should().Be(DateTimeKind.Utc);
        }

        [SkippableFact]
        public async Task AddOrder_StoredDocument_UsesExpectedBsonRepresentations()
        {
            _fixture.SkipIfUnavailable();

            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var gameId = Guid.NewGuid();
            var order = NewOrder(null, (gameId, "Jogo A", 10.55m));
            await repo.AddOrder(order);

            var raw = await db.GetCollection<BsonDocument>("orders").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();

            raw["_id"].BsonType.Should().Be(BsonType.String);
            raw["_id"].AsString.Should().Be(order.Id.ToString());
            raw["customer"]["customerId"].BsonType.Should().Be(BsonType.String);
            raw["games"].AsBsonArray.Single()["gameId"].BsonType.Should().Be(BsonType.String);
            raw["games"].AsBsonArray.Single()["gameAmount"].BsonType.Should().Be(BsonType.Decimal128);
            raw["totalAmount"]["amount"].BsonType.Should().Be(BsonType.Decimal128);
            raw["createdAt"].BsonType.Should().Be(BsonType.DateTime);
            raw["status"].BsonType.Should().Be(BsonType.Int32);
            raw["status"].AsInt32.Should().Be((int)OrderStatus.Draft);
        }

        [SkippableFact]
        public async Task GetOrderById_UnknownId_ReturnsNull()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());

            var loaded = await repo.GetOrderById(Guid.NewGuid(), CancellationToken.None);

            loaded.Should().BeNull();
        }

        [SkippableFact]
        public async Task UpdateOrder_AfterFinalize_PersistsNewStatusAndTotal()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());
            var order = NewOrder(null, (Guid.NewGuid(), "Jogo A", 15m));
            await repo.AddOrder(order);

            var loaded = await repo.GetOrderById(order.Id, CancellationToken.None);
            loaded.FinalizeOrder();
            var updated = await repo.UpdateOrder(loaded);
            var reloaded = await repo.GetOrderById(order.Id, CancellationToken.None);

            updated.Should().BeTrue();
            reloaded.Status.Should().Be(OrderStatus.Completed);
            reloaded.TotalAmount.Amount.Should().Be(15m);
            reloaded.Games.Should().HaveCount(1);
        }

        [SkippableFact]
        public async Task UpdateOrder_AfterCancel_PersistsCancelledStatus()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());
            var order = NewOrder(null, (Guid.NewGuid(), "Jogo A", 15m));
            await repo.AddOrder(order);

            order.CancelOrder();
            await repo.UpdateOrder(order);
            var reloaded = await repo.GetOrderById(order.Id, CancellationToken.None);

            reloaded.Status.Should().Be(OrderStatus.Cancelled);
        }

        [SkippableFact]
        public async Task UpdateOrder_OrderNotPersisted_ReturnsFalse()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());

            var result = await repo.UpdateOrder(NewOrder(null, (Guid.NewGuid(), "Jogo A", 1m)));

            result.Should().BeFalse();
        }

        [SkippableFact]
        public async Task CustomerOwnsGameAsync_CompletedOrderWithGame_ReturnsTrue()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());
            var customer = Guid.NewGuid(); var game = Guid.NewGuid();
            var order = NewOrder(customer, (game, "Jogo A", 10m));
            order.FinalizeOrder();
            await repo.AddOrder(order);

            var owns = await repo.CustomerOwnsGameAsync(customer, game, CancellationToken.None);

            owns.Should().BeTrue();
        }

        [SkippableTheory]
        [InlineData(OrderStatus.Draft)]
        [InlineData(OrderStatus.Cancelled)]
        public async Task CustomerOwnsGameAsync_OrderNotCompleted_ReturnsFalse(OrderStatus status)
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());
            var customer = Guid.NewGuid(); var game = Guid.NewGuid();
            var order = NewOrder(customer, (game, "Jogo A", 10m));
            if (status == OrderStatus.Cancelled) order.CancelOrder();
            await repo.AddOrder(order);

            var owns = await repo.CustomerOwnsGameAsync(customer, game, CancellationToken.None);

            owns.Should().BeFalse();
        }

        [SkippableFact]
        public async Task CustomerOwnsGameAsync_OtherCustomerOrOtherGame_ReturnsFalse()
        {
            _fixture.SkipIfUnavailable();

            var repo = new OrderRepository(_fixture.CreateDatabase());
            var customer = Guid.NewGuid(); var game = Guid.NewGuid();
            var order = NewOrder(customer, (game, "Jogo A", 10m));
            order.FinalizeOrder();
            await repo.AddOrder(order);

            (await repo.CustomerOwnsGameAsync(Guid.NewGuid(), game, CancellationToken.None)).Should().BeFalse();
            (await repo.CustomerOwnsGameAsync(customer, Guid.NewGuid(), CancellationToken.None)).Should().BeFalse();
        }
    }
}
