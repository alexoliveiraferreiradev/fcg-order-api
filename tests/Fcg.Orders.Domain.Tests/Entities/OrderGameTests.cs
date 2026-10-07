using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Orders.Domain.Entities;
using FluentAssertions;

namespace Fcg.Orders.Domain.Tests.Entities
{
    public class OrderGameTests
    {
        [Fact]
        public void Constructor_ShouldCreateOrderGame_WhenDataIsValid()
        {
            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();

            var orderGame = new OrderGame( gameId, "Jogo A", 100m);

            orderGame.GameId.Should().Be(gameId);
            orderGame.GameName.Should().Be("Jogo A");
            orderGame.GameAmount.Should().Be(100m);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenOrderIdIsEmpty()
        {
            var act = () => new OrderGame(Guid.Empty, "Jogo A", 100m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenGameIdIsEmpty()
        {
            var act = () => new OrderGame(Guid.Empty, "Jogo A", 100m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenGameNameIsNull()
        {
            var act = () => new OrderGame(Guid.NewGuid(), null!, 100m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenGameAmountIsNegative()
        {
            var act = () => new OrderGame(Guid.NewGuid(), "Jogo A", -1m);

            act.Should().Throw<DomainException>();
        }
    }
}
