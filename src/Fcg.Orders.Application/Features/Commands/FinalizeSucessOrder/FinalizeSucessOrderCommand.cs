using MediatR;

namespace Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder
{
    public record FinalizeSucessOrderCommand(Guid OrderId, Guid UserId, IEnumerable<Guid> GameIds) : IRequest;
}
