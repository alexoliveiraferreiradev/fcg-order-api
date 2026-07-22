using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.PlaceOrder
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, bool>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;
        private readonly IUserSnapshotLibraryRepository _userLibrarySnapshotRepository;
        private readonly ILogger<PlaceOrderCommandHandler> _logger;
        private readonly IMediator _mediator;

        public PlaceOrderCommandHandler(IPublishEndpoint publishEndpoint, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ILogger<PlaceOrderCommandHandler> logger, IGameSnapshotRepository gameSnapshotRepository, IUserSnapshotLibraryRepository userLibrarySnapshotRepository, IMediator mediator)
        {
            _publishEndpoint = publishEndpoint;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _logger = logger;
            _gameSnapshotRepository = gameSnapshotRepository;
            _userLibrarySnapshotRepository = userLibrarySnapshotRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            if (!request.GamesIds.Any()) throw new DomainException("Não foi solicitado nenhum jogo.");

            var gamesSnapshot = await _gameSnapshotRepository.GetAllSnapshotsByIdsAsync(request.GamesIds);
            var gamesPurchased = await _userLibrarySnapshotRepository.GetPurchasedGamesByUserIdAsync(request.UserId, request.GamesIds);
            var gamesIds = gamesSnapshot.Select(x => x.GameId);
            var missingGames = request.GamesIds.Except(gamesIds);
            var unavailableGames = gamesSnapshot.Where(s => !s.IsAvailableForPurchase).Select(x => x.GameId);

            if (unavailableGames.Any())
            {
                var idsFormatados = string.Join(", ", unavailableGames);
                _logger.LogWarning($"[OrderAPI]  pedido negado. Os seguintes jogos estão indisponíveis para compra: {idsFormatados}");
                throw new DomainException($"A compra foi cancelada. Os seguintes IDS de jogos estão indisponíveis para compra: {idsFormatados}");
            }
            if (missingGames.Any())
            {
                var idsFormatados = string.Join(", ", missingGames);
                _logger.LogWarning($"[OrderAPI]  pedido negado. Os seguintes Jogos não foram encontrados: {idsFormatados}");
                throw new DomainException($"A compra foi cancelada. Os seguintes IDs de Games não foram encontrados no catálogo: {idsFormatados}");
            }

            if (gamesPurchased.Any())
            {
                _logger.LogWarning("[OrderAPI]  pedido negado. Usuário {UsuarioId} já possui o algum dos jogos", request.UserId);
                throw new DomainException($"O usuário já possui algum dos jogos em sua biblioteca.");
            }

            var order = new Order(request.UserId);
            
            foreach (var gameSnapshot in gamesSnapshot)
            {
                order.AddItem(gameSnapshot.GameId, gameSnapshot.Name, gameSnapshot.CurrentPrice);
            }

            order.MakeOrder();

            _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", request.UserId);

            await _mediator.Publish(new OrderEvent(request.UserId));

            _orderRepository.AddOrder(order);

            await _publishEndpoint.Publish<IOrderPlacedEvent>(new
            {
                OrderId = order.Id,
                UserId = order.UserId,
                AmmountPrice = order.TotalAmount,
                GamesIds = order.Games.Select(x => x.GameId)
            }, cancellationToken);

            return await _unitOfWork.CommitAsync();
        }
    }
}
