using MediatR;
using System.ComponentModel;

namespace Fcg.Orders.Application.Features.Commands.PlaceOrder
{
    public record PlaceOrderCommand(
        [property: DefaultValue("00000000-0000-0000-0000-000000000000")] Guid UserId,
        IEnumerable<Guid> GamesIds
    ) : IRequest<bool>;
}
