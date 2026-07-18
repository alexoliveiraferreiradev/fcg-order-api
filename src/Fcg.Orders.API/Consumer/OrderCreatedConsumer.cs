using Fcg.Core.SharedContracts.MessageContracts;
using MassTransit;
using MediatR;


namespace Fcg.Orders.API.Consumer
{
    public class OrderCreatedConsumer : IConsumer<IGameCreatedIntegrationEvent>
    {
        private readonly ISender _sender;
        
        public OrderCreatedConsumer(ISender sender)
        {
            _sender = sender;
        }
        public Task Consume(ConsumeContext<IGameCreatedIntegrationEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
