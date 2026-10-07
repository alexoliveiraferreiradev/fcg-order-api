using MediatR;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.DeactiveGame
{
    public record GameDeactiveCommand(Guid EventId, DateTime CreatedAt, Guid GameId, DateTime OccurredAt) : IRequest;
}
