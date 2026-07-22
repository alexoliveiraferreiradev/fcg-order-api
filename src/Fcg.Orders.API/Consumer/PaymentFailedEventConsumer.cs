using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using MassTransit;
using MediatR;

namespace Fcg.Orders.API.Consumer
{
    public class PaymentFailedEventConsumer : IConsumer<IPaymentFailedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;
        public PaymentFailedEventConsumer(IMediator mediator,
            ILogger<PaymentFailedEventConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IPaymentFailedEvent> context)
        {
            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PaymentFailedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeFailedCommand(order.OrderId, order.Reason);
            await _mediator.Send(finalizeCommand);
        }
    }
}
