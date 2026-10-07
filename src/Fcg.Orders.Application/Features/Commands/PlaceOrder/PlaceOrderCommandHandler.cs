using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.Abstractions.Resources;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Extension;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, bool>
    {
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IOrderRepository _orderRepository;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;
        private readonly ICustomerSnapshotRepository _customerSnapshotRepository;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public PlaceOrderCommandHandler(IIntegrationEventPublisher integrationEventPublisher,
            IOrderRepository orderRepository, ILogger<PlaceOrderCommandHandler> logger,
            IGameSnapshotRepository gameSnapshotRepository,
            ICustomerSnapshotRepository customerSnapshotRepository,
            IMediator mediator,
            IUnitOfWork unitOfWork)
        {
            _integrationEventPublisher = integrationEventPublisher;
            _orderRepository = orderRepository;
            _logger = logger;
            _gameSnapshotRepository = gameSnapshotRepository;
            _customerSnapshotRepository = customerSnapshotRepository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var gamesSnapshot = await _gameSnapshotRepository.GetAllByIdsAsync(request.GamesIds, cancellationToken);

                var gamesIds = gamesSnapshot.Select(x => x.GameId);
                var missingGames = request.GamesIds.Except(gamesIds);
                var unavailableGames = gamesSnapshot.Where(s => !s.IsAvailableForPurchase).Select(x => x.GameId);

                if (unavailableGames.Any())
                {
                    var idsFormatados = string.Join(", ", unavailableGames);
                    _logger.LogWarning(
                        "[OrderAPI] Pedido negado para o Usuário {UserId}. Jogos indisponíveis para compra: {GamesIds}",
                        request.UserId, idsFormatados);
                    throw new DomainException($"A compra foi cancelada. Os seguintes IDS de jogos estão indisponíveis para compra: {idsFormatados}");
                }
                if (missingGames.Any())
                {
                    var idsFormatados = string.Join(", ", missingGames);
                    _logger.LogWarning(
                        "[OrderAPI] Pedido negado para o Usuário {UserId}. Jogos não encontrados no catálogo: {GamesIds}",
                        request.UserId, idsFormatados);
                    throw new DomainException($"A compra foi cancelada. Os seguintes IDs de Games não foram encontrados no catálogo: {idsFormatados}");
                }

                var customerSnapshot = await _customerSnapshotRepository.GetCustomerByIdAsync(request.UserId, cancellationToken);

                if (customerSnapshot == null)
                {
                    _logger.LogWarning("[OrderAPI] Pedido negado, não foi encontrado o Usuário para o {UserId}", request.UserId);
                    throw new DomainException(DomainMessages.UserNotFound);
                }

                var order = new Order(new OrderCustomer(
                    customerSnapshot.CustomerId,
                    customerSnapshot.Name,
                    customerSnapshot.Email));

                foreach (var gameSnapshot in gamesSnapshot)
                {
                    var gamesPurchased = await _orderRepository.CustomerOwnsGameAsync(request.UserId, gameSnapshot.GameId, cancellationToken);

                    if (!gamesPurchased)
                        order.AddItem(gameSnapshot.GameId, gameSnapshot.Name, gameSnapshot.CurrentPrice);
                    else
                    {
                        _logger.LogWarning("[OrderAPI] Pedido negado. Usuário {UserId} já possui o jogo {GameId} na biblioteca.",
                            request.UserId, gameSnapshot.GameId);
                        throw new DomainException($"O usuário já possui algum dos jogos em sua biblioteca.");
                    }
                }

                order.MakeOrder();

                _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", request.UserId);

                await _mediator.Publish(new OrderEvent(request.UserId));

                var guidOrder = await _orderRepository.AddOrder(order);

                _logger.LogInformation("[OrderAPI] Pedido criado para o Usuário: {UserId}, OrderId: {OrderId}", request.UserId, guidOrder);

                await _integrationEventPublisher.PublishAsync<IOrderPlacedIntegrationEvent>(new
                {
                    EventId = Guid.NewGuid(),
                    CreatedAt = order.CreatedAt,
                    OrderId = order.Id,
                    UserId = order.Customer.CustomerId,
                    AmountPrice = order.TotalAmount.Amount,
                    GameIds = order.Games.Select(x => x.GameId).ToList()
                }, cancellationToken);

                OrderMetrics.OrderAttempts.WithLabels("success").Inc();

                return await _unitOfWork.CommitAsync();
            }
            catch(DomainException)
            {
                OrderMetrics.OrderAttempts.WithLabels("failure").Inc();
                throw;
            }
        }
    }
}
