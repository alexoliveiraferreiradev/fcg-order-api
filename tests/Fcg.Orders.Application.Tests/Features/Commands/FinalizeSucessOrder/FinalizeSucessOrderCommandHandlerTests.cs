using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Application.Tests.Fixture;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Orders.Application.Tests.Features.Commands.FinalizeSucessOrder
{
    public class FinalizeSucessOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ILogger<FinalizeSucessOrderCommandHandler>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ICustomerSnapshotRepository> _customerSnapshotRepository = new();
        private readonly Mock<IGameSnapshotRepository> _gameSnapshotRepositoryMock = new();
        private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock = new();
        private OrderCustomerFixture orderCustomerFixture = new();

        public FinalizeSucessOrderCommandHandlerTests()
        {
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(It.IsAny<Order>())).ReturnsAsync(true);
        }

        private FinalizeSucessOrderCommandHandler CreateHandler() => new(
            _orderRepositoryMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object,
            _gameSnapshotRepositoryMock.Object,
            _integrationEventPublisherMock.Object,
            _customerSnapshotRepository.Object,
            _unitOfWorkMock.Object);

        private static GameSnapshot NewGame(string name) =>
            new(Guid.NewGuid(), name, "descricao", "genero", 100m, DateTime.UtcNow);

        private Order ArrangeOrder(params GameSnapshot[] games)
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            foreach (var game in games)
                order.AddItem(game.GameId, game.Name, game.CurrentPrice);

            _orderRepositoryMock.Setup(r => r.GetOrderById(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games);
            _customerSnapshotRepository
                .Setup(r => r.GetCustomerByIdAsync(order.Customer.CustomerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerSnapshot(order.Customer.CustomerId, order.Customer.Name, order.Customer.Email));
            return order;
        }

        private static FinalizeSucessOrderCommand CommandFor(Order order, params GameSnapshot[] games) =>
            new(Guid.NewGuid(), DateTime.UtcNow, order.Id, order.Customer.CustomerId, games.Select(g => g.GameId).ToArray());

        private void VerifyGameUserLibraryPublished(Times times) =>
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameUserLibraryIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), times);

        private void VerifyDeliveryFailedPublished(Times times) =>
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IDeliveryFailedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), times);

        [Fact]
        public async Task Handle_ShouldFinalizeOrderAndAddGamesToLibrary_WhenOrderIsValidAndGamesAreFound()
        {
            var games = new[] { NewGame("Jogo A"), NewGame("Jogo B") };
            var order = ArrangeOrder(games);

            await CreateHandler().Handle(CommandFor(order, games), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Completed);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(order), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            VerifyGameUserLibraryPublished(Times.Exactly(2));
            VerifyDeliveryFailedPublished(Times.Never());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPropagateAndNotCompensate_WhenPublishFailsInTheMiddleOfTheLoop()
        {
            var games = new[] { NewGame("Jogo A"), NewGame("Jogo B") };
            var order = ArrangeOrder(games);
            _integrationEventPublisherMock
                .SetupSequence(p => p.PublishAsync<IGameUserLibraryIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .ThrowsAsync(new TimeoutException("broker indisponível"));

            var act = () => CreateHandler().Handle(CommandFor(order, games), CancellationToken.None);

            await act.Should().ThrowAsync<TimeoutException>();
            VerifyDeliveryFailedPublished(Times.Never());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldRepublishGamesWithoutUpdatingOrCompensating_WhenOrderIsAlreadyCompleted()
        {
            var games = new[] { NewGame("Jogo A"), NewGame("Jogo B") };
            var order = ArrangeOrder(games);
            order.FinalizeOrder();

            await CreateHandler().Handle(CommandFor(order, games), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Completed);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<OrderEvent>(), It.IsAny<CancellationToken>()), Times.Never);
            VerifyGameUserLibraryPublished(Times.Exactly(2));
            VerifyDeliveryFailedPublished(Times.Never());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCompensateWithoutDeliveringAnyGame_WhenAGameSnapshotIsMissing()
        {
            var found = NewGame("Jogo A");
            var missing = NewGame("Jogo B");
            var order = ArrangeOrder(found, missing);
            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { found });

            await CreateHandler().Handle(CommandFor(order, found, missing), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Draft);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);
            VerifyGameUserLibraryPublished(Times.Never());
            VerifyDeliveryFailedPublished(Times.Once());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCompensate_WhenOrderHasNoGames()
        {
            var order = ArrangeOrder();

            await CreateHandler().Handle(CommandFor(order), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Draft);
            VerifyGameUserLibraryPublished(Times.Never());
            VerifyDeliveryFailedPublished(Times.Once());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCompensate_WhenOrderIsCancelled()
        {
            var game = NewGame("Jogo A");
            var order = ArrangeOrder(game);
            order.CancelOrder();

            await CreateHandler().Handle(CommandFor(order, game), CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Cancelled);
            VerifyGameUserLibraryPublished(Times.Never());
            VerifyDeliveryFailedPublished(Times.Once());
        }

        [Fact]
        public async Task Handle_ShouldCompensate_WhenOrderIsNotFound()
        {
            var command = new FinalizeSucessOrderCommand(Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), new[] { Guid.NewGuid() });

            await CreateHandler().Handle(command, CancellationToken.None);

            VerifyGameUserLibraryPublished(Times.Never());
            VerifyDeliveryFailedPublished(Times.Once());
        }

        [Fact]
        public async Task Handle_ShouldThrowWithoutPublishing_WhenOrderUpdateIsNotPersisted()
        {
            var game = NewGame("Jogo A");
            var order = ArrangeOrder(game);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(order)).ReturnsAsync(false);

            var act = () => CreateHandler().Handle(CommandFor(order, game), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            VerifyGameUserLibraryPublished(Times.Never());
            VerifyDeliveryFailedPublished(Times.Never());
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }
    }
}
