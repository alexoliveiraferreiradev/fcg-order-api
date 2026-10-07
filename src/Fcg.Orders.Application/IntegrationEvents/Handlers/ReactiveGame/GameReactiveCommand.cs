using MediatR;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.ReactiveGame
{
    public record GameReactiveCommand(Guid EventId, DateTime CreatedAt, Guid GameId, DateTime OccurredAt) : IRequest;
}
