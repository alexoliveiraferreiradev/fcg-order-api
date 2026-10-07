using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.EventHandler;
using Fcg.Orders.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fcg.Orders.Application.Tests.Features.EventHandler
{
    public class OrderEventHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<ILogger<OrderEventHandler>> _loggerMock = new();

        private OrderEventHandler CreateHandler() => new(_cacheServiceMock.Object, _loggerMock.Object);

        [Fact]
        public async Task Handle_ShouldInvalidateUserCache_WhenNotificationIsValid()
        {
            var userId = Guid.NewGuid();
            var notification = new OrderEvent(userId);

            var handler = CreateHandler();

            await handler.Handle(notification, CancellationToken.None);

            _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync($"order:_u:{userId}"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPropagateException_WhenCacheServiceFails()
        {
            var notification = new OrderEvent(Guid.NewGuid());

            _cacheServiceMock
                .Setup(c => c.RemoveByPrefixAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Falha ao acessar o cache"));

            var handler = CreateHandler();

            var act = async () => await handler.Handle(notification, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
