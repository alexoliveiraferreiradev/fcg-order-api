using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.IntegrationEvents.Handlers.DeactiveGame;
using MassTransit;
using MediatR;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class GameDeactiveConsumer : IConsumer<IGameDeactiveIntegrationEvent>
    {
        private readonly ISender _sender;
        public GameDeactiveConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IGameDeactiveIntegrationEvent> context)
        {
            var message = context.Message;
            var gameCommand = new GameDeactiveCommand(message.EventId,message.CreatedAt,
                message.GameId, message.OccurredAt);
            await _sender.Send(gameCommand, context.CancellationToken);
        }
    }
}
