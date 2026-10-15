using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using Fcg.Orders.Application.Features.Queries.GetOrderHistory;
using Fcg.Orders.Application.IntegrationEvents.Handlers.CreatedGame;
using Fcg.Orders.Application.IntegrationTests.Fixtures;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Infrastructure.Queries;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Fcg.Orders.Application.IntegrationTests.Features.Commands.FinalizeOrder
{
    // Handlers de ponta a ponta contra MongoDB real; apenas portas externas (mediator, broker, UoW, cache) sao mockadas.
    [Collection("Mongo")]
    public class FinalizeOrderHandlersMongoIntegrationTests
    {
        private readonly MongoFixture _fixture;

        public FinalizeOrderHandlersMongoIntegrationTests(MongoFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task FinalizeSuccess_PendingOrder_CompletesOrderInMongoAndPublishesEvents()
        {
            var db = _fixture.CreateDatabase();
            var orderRepo = new OrderRepository(db);
            var gameRepo = new GameSnapshotRepository(db);
            var customerRepo = new CustomerRepository(db);
            var userId = Guid.NewGuid();
            await customerRepo.AddCustomer(new CustomerSnapshot(userId, "Maria", "maria@fcg.com"));
            var game = new GameSnapshot(Guid.NewGuid(), "Hollow Knight", "Metroidvania", "Plataforma", 39.90m, DateTime.UtcNow);
            await gameRepo.AddSnapshot(game);
            var order = new Order(new OrderCustomer(userId, "Maria", "maria@fcg.com"));
            order.AddItem(game.GameId, game.Name, game.CurrentPrice);
            order.MakeOrder();
            await orderRepo.AddOrder(order);

            var mediator = new Mock<IMediator>();
            var publisher = new Mock<IIntegrationEventPublisher>();
            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.CommitAsync()).ReturnsAsync(true);
            var handler = new FinalizeSucessOrderCommandHandler(orderRepo, NullLogger<FinalizeSucessOrderCommandHandler>.Instance,
                mediator.Object, customerRepo, gameRepo, publisher.Object, customerRepo, uow.Object);

            await handler.Handle(new FinalizeSucessOrderCommand(Guid.NewGuid(), DateTime.UtcNow, order.Id, userId, new[] { game.GameId }),
                CancellationToken.None);

            var persisted = await orderRepo.GetOrderById(order.Id, CancellationToken.None);
            persisted.Status.Should().Be(OrderStatus.Completed);
            persisted.TotalAmount.Amount.Should().Be(39.90m);
            mediator.Verify(m => m.Publish(It.IsAny<OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            publisher.Verify(p => p.PublishAsync<IGameUserLibraryIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.CommitAsync(), Times.Once);
            (await orderRepo.CustomerOwnsGameAsync(userId, game.GameId, CancellationToken.None)).Should().BeTrue();
        }

        [Fact]
        public async Task FinalizeFailed_PendingOrder_CancelsOrderInMongo()
        {
            var db = _fixture.CreateDatabase();
            var orderRepo = new OrderRepository(db);
            var order = new Order(new OrderCustomer(Guid.NewGuid(), "Maria", "maria@fcg.com"));
            order.AddItem(Guid.NewGuid(), "Jogo", 10m);
            order.MakeOrder();
            await orderRepo.AddOrder(order);
            var mediator = new Mock<IMediator>();
            var handler = new FinalizeFailedOrderCommandHandler(orderRepo, NullLogger<FinalizeFailedOrderCommandHandler>.Instance, mediator.Object);

            await handler.Handle(new FinalizeFailedCommand(Guid.NewGuid(), DateTime.UtcNow, order.Id, "pagamento recusado"), CancellationToken.None);

            (await orderRepo.GetOrderById(order.Id, CancellationToken.None)).Status.Should().Be(OrderStatus.Cancelled);
            mediator.Verify(m => m.Publish(It.IsAny<OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GameCreated_NewThenExistingGame_InsertsThenUpdatesSnapshot()
        {
            var db = _fixture.CreateDatabase();
            var gameRepo = new GameSnapshotRepository(db);
            var handler = new GameCreatedCommandHandler(gameRepo, NullLogger<GameCreatedCommandHandler>.Instance);
            var gameId = Guid.NewGuid();
            var t0 = DateTime.UtcNow;

            await handler.Handle(new GameCreatedCommand(Guid.NewGuid(), t0, gameId, "Celeste", "Plataforma dificil", "Plataforma", 30m, true, t0), CancellationToken.None);
            var created = await gameRepo.GetByIdAsync(gameId, CancellationToken.None);
            await handler.Handle(new GameCreatedCommand(Guid.NewGuid(), t0, gameId, "Celeste 2", "Descricao nova", "Plataforma", 20m, false, t0.AddMinutes(1)), CancellationToken.None);
            var updated = await gameRepo.GetByIdAsync(gameId, CancellationToken.None);

            created.Name.Should().Be("Celeste");
            created.CurrentPrice.Should().Be(30m);
            updated.Name.Should().Be("Celeste 2");
            updated.CurrentPrice.Should().Be(20m);
            updated.IsAvailableForPurchase.Should().BeFalse();
        }

        [Fact]
        public async Task GetOrderHistory_CompletedAndDraftOrders_MapsPaidPriceByStatus()
        {
            var db = _fixture.CreateDatabase();
            var orderRepo = new OrderRepository(db);
            var userId = Guid.NewGuid();
            var completed = new Order(new OrderCustomer(userId, "Maria", "m@fcg.com"));
            completed.AddItem(Guid.NewGuid(), "Pago", 25m);
            completed.FinalizeOrder();
            await orderRepo.AddOrder(completed);
            await Task.Delay(5);
            var draft = new Order(new OrderCustomer(userId, "Maria", "m@fcg.com"));
            draft.AddItem(Guid.NewGuid(), "Pendente", 40m);
            draft.MakeOrder();
            await orderRepo.AddOrder(draft);

            var cache = new Mock<ICacheService>();
            var handler = new GetOrderHistoryQueryHandler(new OrderQueryRepository(db), cache.Object);

            var result = await handler.Handle(new GetOrderHistoryQuery(userId), CancellationToken.None);

            result.TotalItems.Should().Be(2);
            result.Items.Select(i => i.OrderId).Should().Equal(draft.Id, completed.Id);
            result.Items.First().Items.Single().PaidPrice.Should().Be(0);
            result.Items.Last().Items.Single().PaidPrice.Should().Be(25m);
            result.Items.Last().TotalAmount.Should().Be(25m);
        }
    }
}
