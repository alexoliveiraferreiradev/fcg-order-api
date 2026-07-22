using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.EventHandler
{
    public class OrderEventHandler : INotificationHandler<OrderEvent>
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<OrderEventHandler> _logger;

        public OrderEventHandler(ICacheService cacheService, ILogger<OrderEventHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Handle(OrderEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[OrderAPI] [Cache] Iniciando invalidação de cache da ordem do UsuarioId: {UsuarioId}", notification.UserId);
            await _cacheService.RemoveByPrefixAsync($"order:_u:{notification.UserId}");
            _logger.LogInformation("[OrderAPI] [Cache] Cache de ordem invalidado com sucesso para UsuarioId: {UsuarioId}", notification.UserId);
        }
    }
}
