using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Commands.PlaceOrder;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Orders.Application.Tests.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandHandlerTests
    {
        private readonly Mock<IIntegrationEventPublisher> _integrationEventPublisherMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IGameSnapshotRepository> _gameSnapshotRepositoryMock = new();
        private readonly Mock<ICustomerSnapshotRepository> _userLibrarySnapshotRepositoryMock = new();
        private readonly Mock<ILogger<PlaceOrderCommandHandler>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();

        private PlaceOrderCommandHandler CreateHandler() => new(
            _integrationEventPublisherMock.Object,            
            _orderRepositoryMock.Object,
            _loggerMock.Object,
            _gameSnapshotRepositoryMock.Object,
            _userLibrarySnapshotRepositoryMock.Object,
            _mediatorMock.Object, _unitOfWorkMock.Object);

        private static GameSnapshot BuildSnapshot(Guid gameId, string name, decimal price, bool available = true)
        {
            var snapshot = new GameSnapshot(gameId, name, "descricao", "genero", price, DateTime.UtcNow);

            if (!available)
                snapshot.Update(name, "descricao", price, false, DateTime.UtcNow);

            return snapshot;
        }

        [Fact]
        public async Task Handle_ShouldCreateOrderAndReturnTrue_WhenGamesAreAvailableAndNotOwnedByUser()
        {
            var userId = Guid.NewGuid();
            var gameSnapshot = new GameSnapshot(Guid.NewGuid(), "Jogo A", "descricao", "genero", 100m, DateTime.UtcNow);
            var command = new PlaceOrderCommand(userId, new[] {gameSnapshot.GameId });


            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(command.GamesIds, CancellationToken.None))
                .ReturnsAsync(new[] { gameSnapshot });

            _userLibrarySnapshotRepositoryMock
                .Setup(r => r.GetCustomerByIdAsync(userId, CancellationToken.None))
                .ReturnsAsync(new CustomerSnapshot(userId, "Jogador Teste", "jogador.teste@fcg.com"));

            _orderRepositoryMock
                .Setup(r=>r.CustomerOwnsGameAsync(userId, gameSnapshot.GameId, CancellationToken.None))
                .ReturnsAsync(false);

            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            _orderRepositoryMock.Verify(r => r.AddOrder(It.Is<Domain.Entities.Order>(o => o.Customer.CustomerId == userId && o.TotalAmount.Amount == 100m)), Times.Once);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IOrderPlacedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<Domain.Events.OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainExceptionAndNotCreateOrder_WhenGameIsUnavailableForPurchase()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var command = new PlaceOrderCommand(userId, new[] { gameId });

            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(command.GamesIds, CancellationToken.None))
                .ReturnsAsync(new[] { BuildSnapshot(gameId, "Jogo A", 100m, available: false) });

            _orderRepositoryMock
                .Setup(r => r.CustomerOwnsGameAsync(userId, gameId, CancellationToken.None))
                .ReturnsAsync(false);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _orderRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Domain.Entities.Order>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenGameIsNotFoundInCatalog()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var command = new PlaceOrderCommand(userId, new[] { gameId });

            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(command.GamesIds, CancellationToken.None))
                .ReturnsAsync(Enumerable.Empty<GameSnapshot>());

            _orderRepositoryMock
                .Setup(r => r.CustomerOwnsGameAsync(userId, gameId, CancellationToken.None))
                .ReturnsAsync(false);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _orderRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Domain.Entities.Order>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenGameIsAlreadyOwnedByUser()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var command = new PlaceOrderCommand(userId, new[] { gameId });

            _gameSnapshotRepositoryMock
                .Setup(r => r.GetAllByIdsAsync(command.GamesIds,CancellationToken.None))
                .ReturnsAsync(new[] { BuildSnapshot(gameId, "Jogo A", 100m) });

            _orderRepositoryMock
                .Setup(r => r.CustomerOwnsGameAsync(userId, gameId, CancellationToken.None))
                .ReturnsAsync(true);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _orderRepositoryMock.Verify(r => r.AddOrder(It.IsAny<Domain.Entities.Order>()), Times.Never);
        }
    }
}
