using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.IntegrationEvents.Handlers.CreatedGame;
using MassTransit;
using MediatR;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class GameCreatedConsumer : IConsumer<IGameCreatedIntegrationEvent>
    {        
        private readonly ISender _sender;
        public GameCreatedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IGameCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            var gameCreatedCommand = new GameCreatedCommand(message.EventId,message.CreatedAt,
                message.GameId, message.Name, message.Description,message.Genre,
                message.Price, message.IsAvaiable, message.OccurredAt);
            await _sender.Send(gameCreatedCommand,context.CancellationToken);
        }
    }
}
