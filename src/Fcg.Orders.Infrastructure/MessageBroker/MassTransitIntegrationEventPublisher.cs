using Fcg.Core.SharedContracts.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Infrastructure.MessageBroker
{
    internal class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<MassTransitIntegrationEventPublisher> _logger;

        public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint,
            ILogger<MassTransitIntegrationEventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishAsync<T>(object integrationEvent, CancellationToken cancellationToken = default) where T : class
        {
            var eventName = typeof(T).Name;

            try
            {
                await _publishEndpoint.Publish<T>(integrationEvent, cancellationToken);

                _logger.LogInformation("[OrderAPI] Evento de integração {EventName} enviado para o outbox", eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OrderAPI] Falha ao publicar o evento de integração {EventName}", eventName);
                throw;
            }
        }
    }
}
