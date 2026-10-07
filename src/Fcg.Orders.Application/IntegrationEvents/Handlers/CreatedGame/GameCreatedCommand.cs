using MediatR;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.CreatedGame
{
    public record GameCreatedCommand(Guid EventId, DateTime CreatedAt, Guid GameId, string Name, string Description, string Genre, decimal Price, bool IsAvailable,
            DateTime OccurredAt) : IRequest;
}
