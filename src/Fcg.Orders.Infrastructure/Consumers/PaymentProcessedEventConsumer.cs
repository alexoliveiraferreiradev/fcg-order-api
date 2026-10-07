using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class PaymentProcessedEventConsumer : IConsumer<IPaymentProcessedIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<PaymentProcessedEventConsumer> _logger;

        public PaymentProcessedEventConsumer(ISender sender, ILogger<PaymentProcessedEventConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IPaymentProcessedIntegrationEvent> context)
        {
            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PaymentProcessedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeSucessOrderCommand(order.EventId,order.CreatedAt,
                order.OrderId, order.UserId, order.GameIds);
            await _sender.Send(finalizeCommand, context.CancellationToken);
        }
    }
}
