using Fcg.Orders.Application.IntegrationTests.Fixtures;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Infrastructure.Queries;
using Fcg.Orders.Infrastructure.Repositories;
using FluentAssertions;

namespace Fcg.Orders.Application.IntegrationTests.Infrastructure
{
    [Collection("Mongo")]
    public class OrderQueryRepositoryTests
    {
        private readonly MongoFixture _fixture;

        public OrderQueryRepositoryTests(MongoFixture fixture) => _fixture = fixture;

        private static async Task<Order> SeedAsync(OrderRepository repo, Guid customerId)
        {
            var order = new Order(new OrderCustomer(customerId, "Cliente", "c@fcg.com"));
            order.AddItem(Guid.NewGuid(), "Jogo", 10m);
            order.MakeOrder();
            await repo.AddOrder(order);
            await Task.Delay(5); // garante CreatedAt distintos
            return order;
        }

        [Fact]
        public async Task GetOrderHistoryByUserIdAsync_FiltersByUserAndSortsByCreatedAtDescending()
        {
            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var query = new OrderQueryRepository(db);
            var user = Guid.NewGuid();
            var first = await SeedAsync(repo, user);
            var second = await SeedAsync(repo, user);
            var third = await SeedAsync(repo, user);
            await SeedAsync(repo, Guid.NewGuid());

            var result = await query.GetOrderHistoryByUserIdAsync(user, 1, 10, CancellationToken.None);

            result.TotalItems.Should().Be(3);
            result.Items.Select(o => o.Id).Should().Equal(third.Id, second.Id, first.Id);
        }

        [Fact]
        public async Task GetOrderHistoryByUserIdAsync_Pagination_ReturnsRequestedPage()
        {
            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var query = new OrderQueryRepository(db);
            var user = Guid.NewGuid();
            var o1 = await SeedAsync(repo, user);
            await SeedAsync(repo, user);
            await SeedAsync(repo, user);

            var page2 = await query.GetOrderHistoryByUserIdAsync(user, 2, 2, CancellationToken.None);

            page2.TotalItems.Should().Be(3);
            page2.Items.Select(o => o.Id).Should().Equal(o1.Id);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-3, -5)]
        public async Task GetOrderHistoryByUserIdAsync_InvalidPageOrSize_IsClamped(int page, int size)
        {
            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var query = new OrderQueryRepository(db);
            var user = Guid.NewGuid();
            await SeedAsync(repo, user);
            await SeedAsync(repo, user);

            var result = await query.GetOrderHistoryByUserIdAsync(user, page, size, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalItems.Should().Be(2);
        }

        [Fact]
        public async Task GetOrderHistoryByUserIdAsync_PageSizeAboveMax_IsClampedToMax()
        {
            var db = _fixture.CreateDatabase();
            var repo = new OrderRepository(db);
            var query = new OrderQueryRepository(db);
            var user = Guid.NewGuid();
            for (var i = 0; i < OrderQueryRepository.MaxPageSize + 5; i++)
                await repo.AddOrder(new Order(new OrderCustomer(user, "C", "c@fcg.com")));

            var result = await query.GetOrderHistoryByUserIdAsync(user, 1, 10_000, CancellationToken.None);

            result.Items.Should().HaveCount(OrderQueryRepository.MaxPageSize);
            result.TotalItems.Should().Be(OrderQueryRepository.MaxPageSize + 5);
        }

        [Fact]
        public async Task GetOrderHistoryByUserIdAsync_UserWithoutOrders_ReturnsEmpty()
        {
            var query = new OrderQueryRepository(_fixture.CreateDatabase());

            var result = await query.GetOrderHistoryByUserIdAsync(Guid.NewGuid(), 1, 10, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
        }
    }
}
