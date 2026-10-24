using Fcg.Orders.Application.IntegrationTests.Fixtures;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Fcg.Orders.Application.IntegrationTests.Infrastructure
{
    [Collection("Mongo")]
    public class GameSnapshotRepositoryTests
    {
        private readonly MongoFixture _fixture;

        public GameSnapshotRepositoryTests(MongoFixture fixture) => _fixture = fixture;

        private static GameSnapshot NewGame(string name = "The Witcher 3", decimal price = 59.90m) =>
            new(Guid.NewGuid(), name, "Descricao do jogo", "RPG", price, DateTime.UtcNow);

        [Fact]
        public async Task AddSnapshot_GetByIdAsync_RoundTripPreservesAllFields()
        {
            var db = _fixture.CreateDatabase();
            var repo = new GameSnapshotRepository(db);
            var game = NewGame();

            var id = await repo.AddSnapshot(game);
            var loaded = await repo.GetByIdAsync(id, CancellationToken.None);

            loaded.GameId.Should().Be(game.GameId);
            loaded.Name.Should().Be(game.Name);
            loaded.Description.Should().Be(game.Description);
            loaded.Genre.Should().Be("RPG");
            loaded.CurrentPrice.Should().Be(59.90m);
            loaded.IsAvailableForPurchase.Should().BeTrue();
            loaded.LastSyncedAt.Should().BeCloseTo(game.LastSyncedAt, TimeSpan.FromMilliseconds(1));
            loaded.LastSyncedAt.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public async Task AddSnapshot_StoredDocument_UsesIdAsStringAndPriceAsDecimal128()
        {
            var db = _fixture.CreateDatabase();
            var repo = new GameSnapshotRepository(db);
            var game = NewGame();
            await repo.AddSnapshot(game);

            var raw = await db.GetCollection<BsonDocument>("games").Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();

            raw["_id"].BsonType.Should().Be(BsonType.String);
            raw["_id"].AsString.Should().Be(game.GameId.ToString());
            raw["currentPrice"].BsonType.Should().Be(BsonType.Decimal128);
        }

        [Fact]
        public async Task GetByIdAsync_UnknownId_ReturnsNull()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());

            (await repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None)).Should().BeNull();
        }

        [Fact]
        public async Task GetAllByIdsAsync_SubsetOfIds_ReturnsOnlyMatching()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());
            var a = NewGame("Jogo A"); var b = NewGame("Jogo B"); var c = NewGame("Jogo C");
            await repo.AddSnapshot(a); await repo.AddSnapshot(b); await repo.AddSnapshot(c);

            var result = (await repo.GetAllByIdsAsync(new[] { a.GameId, c.GameId, Guid.NewGuid() }, CancellationToken.None)).ToList();

            result.Select(x => x.GameId).Should().BeEquivalentTo(new[] { a.GameId, c.GameId });
        }

        [Fact]
        public async Task GetAllByIdsAsync_EmptyInput_ReturnsEmpty()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());
            await repo.AddSnapshot(NewGame());

            var result = await repo.GetAllByIdsAsync(Array.Empty<Guid>(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateSnapshot_ChangedGame_PersistsChanges()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());
            var game = NewGame();
            await repo.AddSnapshot(game);

            game.Update("Novo Nome", "Nova descricao", 10m, false, game.LastSyncedAt.AddMinutes(1));
            var ok = await repo.UpdateSnapshot(game);
            var loaded = await repo.GetByIdAsync(game.GameId, CancellationToken.None);

            ok.Should().BeTrue();
            loaded.Name.Should().Be("Novo Nome");
            loaded.Description.Should().Be("Nova descricao");
            loaded.CurrentPrice.Should().Be(10m);
            loaded.IsAvailableForPurchase.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateSnapshot_NotPersisted_ReturnsFalse()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());

            (await repo.UpdateSnapshot(NewGame())).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteSnapshot_ExistingGame_RemovesIt()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());
            var game = NewGame();
            await repo.AddSnapshot(game);

            var deleted = await repo.DeleteSnapshot(game);

            deleted.Should().BeTrue();
            (await repo.GetByIdAsync(game.GameId, CancellationToken.None)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteSnapshot_NotPersisted_ReturnsFalse()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());

            (await repo.DeleteSnapshot(NewGame())).Should().BeFalse();
        }

        [Fact]
        public async Task AddSnapshot_DuplicateId_ThrowsMongoWriteException()
        {
            var repo = new GameSnapshotRepository(_fixture.CreateDatabase());
            var game = NewGame();
            await repo.AddSnapshot(game);

            var act = async () => await repo.AddSnapshot(game);

            await act.Should().ThrowAsync<MongoWriteException>();
        }
    }
}
