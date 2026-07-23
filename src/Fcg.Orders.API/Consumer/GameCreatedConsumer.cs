using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.IntegrationEvents.Handlers;
using MassTransit;

namespace Fcg.Orders.API.Consumer
{
    public class GameCreatedConsumer : IConsumer<IGameCreatedIntegrationEvent>
    {
        private readonly GameCreatedHandler _handler;
        public GameCreatedConsumer(GameCreatedHandler handler)
        {
            _handler = handler;
        }

        public async Task Consume(ConsumeContext<IGameCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            await _handler.Handle(message.GameId, message.Name, message.Description,message.Genre,
                message.Price, message.IsAvaiable, message.CreatedAt);
        }
    }
}
