using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Orders.Application.Features.Commands.PlaceOrder;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Infrastructure.Persistence.Mongo;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace Fcg.Orders.Application.IntegrationTests.Features.Commands.PlaceOrder
{
    // Testes manuais contra um MongoDB real.
    public class PlaceOrderCommandHandlerMongoIntegrationTests
    {
        private static IMongoDatabase CreateDatabase()
        {
            MongoConventions.EnsureRegistered();
            var client = new MongoClient("mongodb://admin:TechChallenge%402026@localhost:27017/?directConnection=true&authSource=admin");
            return client.GetDatabase($"fcg_orders_playtest_{Guid.NewGuid():N}");
        }

        private static PlaceOrderCommandHandler CreateHandler(
            OrderRepository orderRepo,
            GameSnapshotRepository gameRepo,
            CustomerRepository customerRepo)
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            return new PlaceOrderCommandHandler(
                new Mock<IIntegrationEventPublisher>().Object,
                orderRepo,
                new Mock<ILogger<PlaceOrderCommandHandler>>().Object,
                gameRepo,
                customerRepo,
                new Mock<IMediator>().Object,
                unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldPersistOrderInMongo_WhenSimulatingRealUserPurchase()
        {
            var database = CreateDatabase();
            var gameRepo = new GameSnapshotRepository(database);
            var customerRepo = new CustomerRepository(database);
            var orderRepo = new OrderRepository(database);

            var userId = Guid.NewGuid();
            await customerRepo.AddCustomer(new CustomerSnapshot(userId, "Jogador Teste", "jogador.teste@fcg.com"));

            var witcher = new GameSnapshot(Guid.NewGuid(), "The Witcher 3", "RPG de mundo aberto", "RPG", 59.90m, DateTime.UtcNow);
            var hollowKnight = new GameSnapshot(Guid.NewGuid(), "Hollow Knight", "Metroidvania desenhado a mao", "Plataforma", 39.90m, DateTime.UtcNow);
            await gameRepo.AddSnapshot(witcher);
            await gameRepo.AddSnapshot(hollowKnight);

            var command = new PlaceOrderCommand(userId, new[] { witcher.GameId, hollowKnight.GameId });
            var handler = CreateHandler(orderRepo, gameRepo, customerRepo);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();

            var ordersCollection = database.GetCollection<Order>("orders");
            var persistedOrder = await ordersCollection
                .Find(o => o.Customer.CustomerId == userId)
                .SingleAsync();

            persistedOrder.Games.Should().HaveCount(2);
            persistedOrder.TotalAmount.Amount.Should().Be(99.80m);
        }

        [Fact]
        public async Task Handle_ShouldRejectSecondPurchase_WhenUserAlreadyOwnsCompletedGame()
        {
            var database = CreateDatabase();
            var gameRepo = new GameSnapshotRepository(database);
            var customerRepo = new CustomerRepository(database);
            var orderRepo = new OrderRepository(database);

            var userId = Guid.NewGuid();
            await customerRepo.AddCustomer(new CustomerSnapshot(userId, "Jogador Teste", "jogador.teste@fcg.com"));

            var stardewValley = new GameSnapshot(Guid.NewGuid(), "Stardew Valley", "Simulador de fazenda", "Simulacao", 24.90m, DateTime.UtcNow);
            await gameRepo.AddSnapshot(stardewValley);

            var previousOrder = new Order(new OrderCustomer(userId, "Jogador Teste", "jogador.teste@fcg.com"));
            previousOrder.AddItem(stardewValley.GameId, stardewValley.Name, stardewValley.CurrentPrice);
            previousOrder.FinalizeOrder();
            await orderRepo.AddOrder(previousOrder);

            var command = new PlaceOrderCommand(userId, new[] { stardewValley.GameId });
            var handler = CreateHandler(orderRepo, gameRepo, customerRepo);

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
