using MediatR;

namespace Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder
{
    public record FinalizeFailedCommand(Guid OrderId, string ReasonFailed) : IRequest;
}
