using MediatR;

namespace Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder
{
    public record FinalizeFailedCommand(Guid EventId, DateTime CreatedAt, Guid OrderId, string ReasonFailed) : IRequest;
}
