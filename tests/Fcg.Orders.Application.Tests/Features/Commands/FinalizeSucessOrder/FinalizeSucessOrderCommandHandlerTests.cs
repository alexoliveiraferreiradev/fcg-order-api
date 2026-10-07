using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Application.Tests.Fixture;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
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
        private FinalizeSucessOrderCommandHandler CreateHandler() => new(
            _orderRepositoryMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object,
            _customerSnapshotRepository.Object,
            _gameSnapshotRepositoryMock.Object,
            _integrationEventPublisherMock.Object,
            _customerSnapshotRepository.Object,
            _unitOfWorkMock.Object);

        [Fact]
        public async Task Handle_ShouldFinalizeOrderAndAddGamesToLibrary_WhenOrderIsValidAndGamesAreFound()
        {
            var userId = Guid.NewGuid();
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            var snapshot = new GameSnapshot(Guid.NewGuid(), "Jogo A", "descricao", "genero", 100m, DateTime.UtcNow);
            order.AddItem(snapshot.GameId, "Jogo A", 100m);
            var command = new FinalizeSucessOrderCommand(Guid.NewGuid(),DateTime.Now,order.Id, userId, new[] { snapshot.GameId });


            _orderRepositoryMock.Setup(r => r.GetOrderById(order.Id,CancellationToken.None)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(order)).ReturnsAsync(true);
            _gameSnapshotRepositoryMock.Setup(r => r.GetByIdAsync(snapshot.GameId,CancellationToken.None)).ReturnsAsync(snapshot);
            _customerSnapshotRepository
                .Setup(r => r.GetCustomerByIdAsync(order.Customer.CustomerId, CancellationToken.None))
                .ReturnsAsync(new CustomerSnapshot(order.Customer.CustomerId, order.Customer.Name, order.Customer.Email));
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Completed);
            _customerSnapshotRepository.Verify(r => r.GetCustomerByIdAsync(order.Customer.CustomerId, CancellationToken.None), Times.Once);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IGameUserLibraryIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<Domain.Events.OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IDeliveryFailedIntegrationEvent>(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldPublishDeliveryFailedAndStillCommit_WhenOrderCannotBeFinalized()
        {
            var userId = Guid.NewGuid();
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            var command = new FinalizeSucessOrderCommand(Guid.NewGuid(),DateTime.Now,order.Id, userId, Enumerable.Empty<Guid>());

            _orderRepositoryMock.Setup(r => r.GetOrderById(order.Id,CancellationToken.None)).ReturnsAsync(order);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Draft);
            _integrationEventPublisherMock.Verify(p => p.PublishAsync<IDeliveryFailedIntegrationEvent>(
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
