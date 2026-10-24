using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class PaymentApprovedEventConsumer : IConsumer<IPaymentApprovedIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<PaymentApprovedEventConsumer> _logger;

        public PaymentApprovedEventConsumer(ISender sender, ILogger<PaymentApprovedEventConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IPaymentApprovedIntegrationEvent> context)
        {
            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PaymentApprovedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeSucessOrderCommand(order.EventId,order.CreatedAt,
                order.OrderId, order.UserId, order.GameIds);
            await _sender.Send(finalizeCommand, context.CancellationToken);
        }
    }
}
