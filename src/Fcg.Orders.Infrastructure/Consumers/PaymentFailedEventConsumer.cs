using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<IPaymentFailedIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;
        public PaymentFailedEventConsumer(ISender sender,
            ILogger<PaymentFailedEventConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IPaymentFailedIntegrationEvent> context)
        {
            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PaymentFailedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeFailedCommand(order.EventId,order.CreatedAt,
                order.OrderId, order.Reason);
            await _sender.Send(finalizeCommand, context.CancellationToken);
        }
    }
}
