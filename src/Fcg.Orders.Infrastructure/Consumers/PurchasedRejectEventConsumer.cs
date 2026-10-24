using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class PurchasedRejectEventConsumer : IConsumer<IPurchaseRejectedIntegrationEvent>
    {
        private readonly ISender _sender;
        private readonly ILogger<PurchasedRejectEventConsumer> _logger;
        public PurchasedRejectEventConsumer(ISender sender,
            ILogger<PurchasedRejectEventConsumer> logger)
        {
            _sender = sender;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IPurchaseRejectedIntegrationEvent> context)
        {
            var order = context.Message;
            _logger.LogInformation("[OrderAPI] PurchaseRejectedEvent recebido para o Pedido {OrderId}", order.OrderId);
            var finalizeCommand = new FinalizeFailedCommand(order.EventId,order.CreatedAt,
                order.OrderId, order.Reason);
            await _sender.Send(finalizeCommand, context.CancellationToken);
        }
    }
}
