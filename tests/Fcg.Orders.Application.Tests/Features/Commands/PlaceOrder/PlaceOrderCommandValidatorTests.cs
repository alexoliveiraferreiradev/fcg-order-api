using Fcg.Orders.Application.Features.Commands.PlaceOrder;
using FluentAssertions;

namespace Fcg.Orders.Application.Tests.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandValidatorTests
    {
        private readonly PlaceOrderCommandValidator _validator = new();

        [Fact]
        public void Validate_ShouldBeValid_WhenGamesIdsListIsNotEmpty()
        {
            var command = new PlaceOrderCommand(Guid.NewGuid(), new[] { Guid.NewGuid() });

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenGamesIdsListIsEmpty()
        {
            var command = new PlaceOrderCommand(Guid.NewGuid(), Enumerable.Empty<Guid>());

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(PlaceOrderCommand.GamesIds));
        }
    }
}
