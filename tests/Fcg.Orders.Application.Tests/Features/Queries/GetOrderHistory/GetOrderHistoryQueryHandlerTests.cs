using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Queries.GetOrderHistory;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Fcg.Orders.Application.Tests.Features.Queries.GetOrderHistory
{
    public class GetOrderHistoryQueryHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IOrderQueryRepository> _orderQueryRepositoryMock = new();

        private GetOrderHistoryQueryHandler CreateHandler() => new(
            _orderQueryRepositoryMock.Object,
            _cacheServiceMock.Object);

        [Fact]
        public async Task Handle_ShouldReturnCachedResultWithoutQueryingRepository_WhenCacheExists()
        {
            var query = new GetOrderHistoryQuery(Guid.NewGuid());
            var cached = new PagedResult<OrderHistoryResponse>(new List<OrderHistoryResponse> { new() }, query.Page, query.PageSize, 1);

            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<OrderHistoryResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cached);

            var handler = CreateHandler();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().BeSameAs(cached);
            _orderQueryRepositoryMock.Verify(r => r.GetOrderHistoryByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldQueryRepositoryAndCacheResult_WhenCacheDoesNotExistAndHasResults()
        {
            var query = new GetOrderHistoryQuery(Guid.NewGuid());
            var orderCustomer = new OrderCustomer(Guid.NewGuid(), "John Doe", "john.doe@example.com");
            var pagedResult = new PagedResult<Order>(new List<Order> { new(orderCustomer) }, query.Page, query.PageSize, 1);
            var pagedResultHistory =  new PagedResult<OrderHistoryResponse>(new List<OrderHistoryResponse> { new() }, query.Page, query.PageSize, 1);
            
            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<OrderHistoryResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PagedResult<OrderHistoryResponse>?)null);

            _orderQueryRepositoryMock
                .Setup(r => r.GetOrderHistoryByUserIdAsync(query.UserId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            pagedResultHistory = new PagedResult<OrderHistoryResponse>(pagedResult.Items.Select(o => new OrderHistoryResponse
            {
                OrderId = o.Id,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount.Amount,
                Items = o.Games.Select(g => new OrderItemResponse
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    PaidPrice = g.GameAmount
                }).ToList()
            }).ToList(), query.Page, query.PageSize, pagedResult.Items.Count());

            var handler = CreateHandler();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().BeEquivalentTo(pagedResultHistory);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), pagedResult, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCacheResult_WhenCacheDoesNotExistAndRepositoryReturnsNoItems()
        {
            var query = new GetOrderHistoryQuery(Guid.NewGuid());
            var orderCustomer = new OrderCustomer(Guid.NewGuid(), "John Doe", "john.doe@example.com");
            var pagedResult = new PagedResult<Order>(new List<Order>(), query.Page, query.PageSize, 0);

            _cacheServiceMock
                .Setup(c => c.GetAsync<PagedResult<OrderHistoryResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PagedResult<OrderHistoryResponse>?)null);

            _orderQueryRepositoryMock
                .Setup(r => r.GetOrderHistoryByUserIdAsync(query.UserId, query.Page, query.PageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var handler = CreateHandler();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Items.Should().BeEmpty();
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResult<OrderHistoryResponse>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
