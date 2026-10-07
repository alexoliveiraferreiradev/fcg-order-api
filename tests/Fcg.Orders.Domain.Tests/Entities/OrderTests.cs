using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Tests.Fixture;
using FluentAssertions;

namespace Fcg.Orders.Domain.Tests.Entities
{
    public class OrderTests
    {
        private OrderCustomerFixture orderCustomerFixture = new OrderCustomerFixture();
        [Fact]
        public void Constructor_ShouldCreateOrderInDraft_WhenUserIdIsValid()
        {

            var order = new Order(orderCustomerFixture.GetSucessCustomer());

            order.Status.Should().Be(OrderStatus.Draft);
            order.Games.Should().BeEmpty();
            order.TotalAmount.Amount.Should().Be(0);
        }


        [Fact]
        public void Constructor_ShouldNotCreateOrderInDraft_WhenUserIdIsValid()
        {
            var act = () => new Order(orderCustomerFixture.GetMissingCustomer());
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AddItem_ShouldAddItemToOrder_WhenDataIsValid()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            var gameId = Guid.NewGuid();

            order.AddItem(gameId, "Jogo A", 100m);

            order.Games.Should().ContainSingle(g => g.GameId == gameId && g.GameName == "Jogo A" && g.GameAmount == 100m);
        }

        [Fact]
        public void AddItem_ShouldThrowDomainException_WhenGameIdIsEmpty()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());

            var act = () => order.AddItem(Guid.Empty, "Jogo A", 100m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AddItem_ShouldThrowDomainException_WhenGameIsAlreadyAdded()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            var gameId = Guid.NewGuid();
            order.AddItem(gameId, "Jogo A", 100m);

            var act = () => order.AddItem(gameId, "Jogo A", 100m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void AddItem_ShouldThrowDomainException_WhenOrderIsNotInDraft()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.AddItem(Guid.NewGuid(), "Jogo A", 100m);
            order.FinalizeOrder();

            var act = () => order.AddItem(Guid.NewGuid(), "Jogo B", 50m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void MakeOrder_ShouldCalculateTotalAmount_BasedOnItems()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.AddItem(Guid.NewGuid(), "Jogo A", 100m);
            order.AddItem(Guid.NewGuid(), "Jogo B", 50m);

            order.MakeOrder();

            order.TotalAmount.Amount.Should().Be(150m);
        }

        [Fact]
        public void CancelOrder_ShouldChangeStatusToCancelled_WhenOrderIsInDraft()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());

            order.CancelOrder();

            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void CancelOrder_ShouldThrowDomainException_WhenOrderIsNotInDraft()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.CancelOrder();

            var act = () => order.CancelOrder();

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void FinalizeOrder_ShouldChangeStatusAndCalculateTotal_WhenOrderHasItems()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.AddItem(Guid.NewGuid(), "Jogo A", 100m);

            order.FinalizeOrder();

            order.Status.Should().Be(OrderStatus.Completed);
            order.TotalAmount.Amount.Should().Be(100m);
        }

        [Fact]
        public void FinalizeOrder_ShouldThrowDomainException_WhenOrderHasNoItems()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());

            var act = () => order.FinalizeOrder();

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void FinalizeOrder_ShouldThrowDomainException_WhenOrderIsNotInDraft()
        {
            var order = new Order(orderCustomerFixture.GetSucessCustomer());
            order.AddItem(Guid.NewGuid(), "Jogo A", 100m);
            order.FinalizeOrder();

            var act = () => order.FinalizeOrder();

            act.Should().Throw<DomainException>();
        }
    }
}
