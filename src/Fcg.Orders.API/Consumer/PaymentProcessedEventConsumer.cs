using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using MassTransit;
using MediatR;

namespace Fcg.Orders.API.Consumer
{
    public class PaymentProcessedEventConsumer : IConsumer<IPaymentProcessedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentProcessedEventConsumer> _logger;

        public PaymentProcessedEventConsumer(IMediator mediator, ILogger<PaymentProcessedEventConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IPaymentProcessedEvent> context)
        {

            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PaymentProcessedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeSucessOrderCommand(order.OrderId, order.UserId, order.GameIds);
            await _mediator.Send(finalizeCommand);
        }
    }
}
