using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder
{
    // Falhas de infraestrutura não são capturadas: propagam para o UseMessageRetry e, esgotadas as tentativas, para a fila de erro.
    // A compensação (DeliveryFailed) só é publicada quando o pedido não pode ser entregue, e sempre antes de qualquer GameUserLibrary.
    public class FinalizeSucessOrderCommandHandler : IRequestHandler<FinalizeSucessOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<FinalizeSucessOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;
        private readonly ICustomerSnapshotRepository _customerSnapshotRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FinalizeSucessOrderCommandHandler(IOrderRepository orderRepository,
            ILogger<FinalizeSucessOrderCommandHandler> logger, IMediator mediator,
            IGameSnapshotRepository gameSnapshotRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            ICustomerSnapshotRepository customerSnapshotRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _mediator = mediator;
            _customerSnapshotRepository = customerSnapshotRepository;
            _gameSnapshotRepository = gameSnapshotRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(FinalizeSucessOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[OrderAPI] Pagamento aprovado para o Pedido: {OrderId}. Adicionando Jogos à Biblioteca do Usuário: {UserId}", request.OrderId, request.UserId);

            var order = await _orderRepository.GetOrderById(request.OrderId, cancellationToken);
            if (order is null)
            {
                await PublishDeliveryFailedAsync(request, "Pedido não encontrado.", cancellationToken);
                return;
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                await PublishDeliveryFailedAsync(request, "Pedido cancelado não pode ser entregue.", cancellationToken);
                return;
            }

            var gameIds = request.GameIds.Distinct().ToList();
            var games = (await _gameSnapshotRepository.GetAllByIdsAsync(gameIds, cancellationToken)).ToList();
            var customer = await _customerSnapshotRepository.GetCustomerByIdAsync(order.Customer.CustomerId, cancellationToken);

            if (!order.Games.Any() || gameIds.Count == 0 || games.Count != gameIds.Count || customer is null)
            {
                await PublishDeliveryFailedAsync(request, "Jogos ou cliente do pedido não encontrados para entrega.", cancellationToken);
                return;
            }

            if (order.Status == OrderStatus.Draft)
            {
                order.FinalizeOrder();

                if (!await _orderRepository.UpdateOrder(order))
                    throw new InvalidOperationException($"Não foi possível atualizar o Pedido {order.Id} para Completed.");

                _logger.LogInformation("[OrderAPI] Pedido {OrderId} do Usuário {UserId} aprovado com sucesso.", order.Id, customer.CustomerId);
                await _mediator.Publish(new OrderEvent(customer.CustomerId), cancellationToken);
            }
            else
            {
                _logger.LogInformation("[OrderAPI] Pedido {OrderId} já está Completed. Republicando a entrega dos jogos.", order.Id);
            }

            foreach (var game in games)
            {
                await _integrationEventPublisher.PublishAsync<IGameUserLibraryIntegrationEvent>(new
                {
                    EventId = request.EventId,
                    CreatedAt = request.CreatedAt,
                    UserId = customer.CustomerId,
                    GameId = game.GameId,
                    Name = game.Name,
                    IsAvaiable = true,
                    Description = game.Description,
                    Genre = game.Genre,
                    OccurredAt = order.UpdatedAt
                }, cancellationToken);
            }

            await _unitOfWork.CommitAsync();
        }

        private async Task PublishDeliveryFailedAsync(FinalizeSucessOrderCommand request, string reason, CancellationToken cancellationToken)
        {
            _logger.LogWarning("[OrderAPI] Pedido {OrderId} do Usuário {UserId} não pode ser entregue: {Reason}. Publicando DeliveryFailedEvent para compensação.",
                request.OrderId, request.UserId, reason);

            await _integrationEventPublisher.PublishAsync<IDeliveryFailedIntegrationEvent>(new
            {
                EventId = request.EventId,
                CreatedAt = request.CreatedAt,
                OrderId = request.OrderId,
                UserId = request.UserId,
                Reason = reason
            }, cancellationToken);

            await _unitOfWork.CommitAsync();
        }
    }
}
