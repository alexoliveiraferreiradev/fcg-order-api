using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using Fcg.Orders.Application.Tests.Fixture;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Orders.Application.Tests.Features.Commands.FinalizeFailedOrder
{
    public class FinalizeFailedOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<ILogger<FinalizeFailedOrderCommandHandler>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private OrderCustomerFixture orderCustomerFixture = new OrderCustomerFixture();
        private FinalizeFailedOrderCommandHandler CreateHandler() => new(
            _orderRepositoryMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object);

        [Fact]
        public async Task Handle_ShouldCancelOrderAndCommit_WhenOrderIsInDraft()
        {            
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            var command = new FinalizeFailedCommand(Guid.NewGuid(),DateTime.Now,order.Id, "Pagamento recusado");

            _orderRepositoryMock.Setup(r => r.GetOrderById(order.Id, CancellationToken.None)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(r => r.UpdateOrder(order)).ReturnsAsync(true);

            var handler = CreateHandler();

            await handler.Handle(command, CancellationToken.None);

            order.Status.Should().Be(OrderStatus.Cancelled);
            _orderRepositoryMock.Verify(r => r.UpdateOrder(order), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<Domain.Events.OrderEvent>(), It.IsAny<CancellationToken>()), Times.Once);         
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainExceptionAndNotCommit_WhenOrderIsNotInDraft()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.CancelOrder();
            var command = new FinalizeFailedCommand(Guid.NewGuid(),DateTime.Now,order.Id, "Pagamento recusado");

            _orderRepositoryMock.Setup(r => r.GetOrderById(order.Id, CancellationToken.None)).ReturnsAsync(order);

            var handler = CreateHandler();

            var act = async () => await handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<DomainException>();
            _orderRepositoryMock.Verify(r => r.UpdateOrder(It.IsAny<Order>()), Times.Never);            
        }
    }
}
