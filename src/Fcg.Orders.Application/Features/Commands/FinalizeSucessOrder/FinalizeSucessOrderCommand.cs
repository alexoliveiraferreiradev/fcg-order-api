using MediatR;

namespace Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder
{
    public record FinalizeSucessOrderCommand(Guid EventId, DateTime CreatedAt, Guid OrderId, Guid UserId, IEnumerable<Guid> GameIds) : IRequest;
}
