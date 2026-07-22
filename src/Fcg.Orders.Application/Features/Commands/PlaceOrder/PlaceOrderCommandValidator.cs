using FluentValidation;

namespace Fcg.Orders.Application.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.GamesIds).NotEmpty().WithMessage("A lista de Games adquiridos não pode estar vazia.");
        }
    }
}
