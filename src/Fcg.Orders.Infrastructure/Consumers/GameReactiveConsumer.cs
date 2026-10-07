using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.IntegrationEvents.Handlers.ReactiveGame;
using MassTransit;
using MediatR;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class GameReactiveConsumer : IConsumer<IGameReactiveIntegrationEvent>
    {
        private readonly ISender _sender;
        public GameReactiveConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IGameReactiveIntegrationEvent> context)
        {
            var message = context.Message;  
            var gameReactiveCommand = new GameReactiveCommand(message.EventId,message.CreatedAt,
                message.GameId, message.OccurredAt);
            await _sender.Send(gameReactiveCommand, context.CancellationToken);
        }
    }
}
