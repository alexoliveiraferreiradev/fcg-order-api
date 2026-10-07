using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Orders.Domain.ValueObject;
using FluentAssertions;

namespace Fcg.Orders.Domain.Tests.ValueObject
{
    public class PriceTests
    {
        [Fact]
        public void Constructor_ShouldCreatePrice_WhenAmountIsValid()
        {
            var price = new Price(100m);

            price.Amount.Should().Be(100m);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenAmountIsNegative()
        {
            var act = () => new Price(-1m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Equals_ShouldBeConsideredEqual_WhenAmountsAreTheSame()
        {
            var price1 = new Price(100m);
            var price2 = new Price(100m);

            price1.Equals(price2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldNotBeConsideredEqual_WhenAmountsAreDifferent()
        {
            var price1 = new Price(100m);
            var price2 = new Price(200m);

            price1.Equals(price2).Should().BeFalse();
        }
    }
}
