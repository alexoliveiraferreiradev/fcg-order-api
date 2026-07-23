using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder
{
    public class FinalizeSucessOrderCommandHandler : IRequestHandler<FinalizeSucessOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinalizeSucessOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IUserSnapshotLibraryRepository _userLibrarySnapshotRepository;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;

        public FinalizeSucessOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<FinalizeSucessOrderCommandHandler> logger, IMediator mediator, IPublishEndpoint publishEndpoint, IUserSnapshotLibraryRepository userLibrarySnapshotRepository, IGameSnapshotRepository gameSnapshotRepository)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
            _publishEndpoint = publishEndpoint;
            _userLibrarySnapshotRepository = userLibrarySnapshotRepository;
            _gameSnapshotRepository = gameSnapshotRepository;
        }

        public async Task Handle(FinalizeSucessOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;
                var orderUser = await _orderRepository.GetOrderById(request.OrderId);
                _logger.LogInformation("[OrderAPI] Pagamento aprovado para o Pedido: {OrderId}. Adicionando Jogos à Biblioteca do Usuário: {UserId}", request.OrderId, request.UserId);

                foreach (var guidJogo in request.GameIds)
                {
                    var gamesnapshot = await _gameSnapshotRepository.GetSnapshotByIdAsync(guidJogo);
                    var librarySnapshot = new UserLibrarySnapshot(orderUser.UserId, gamesnapshot.GameId);
                    _userLibrarySnapshotRepository.AddUserLibrary(librarySnapshot);

                    await _publishEndpoint.Publish<IGameUserLibraryEvent>(new
                    {
                        UserId = orderUser.Id,
                        GameId = gamesnapshot.GameId,
                        Name = gamesnapshot.Name,
                        IsAvaiable = true,
                        Description = gamesnapshot.Description,
                        Genre = gamesnapshot.Genre,
                        OccurredAt = now
                    });

                }              

                orderUser.FinalizeOrder();

                _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", request.UserId);

                await _mediator.Publish(new OrderEvent(request.UserId));                              

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _publishEndpoint.Publish<IDeliveryFailedEvent>(new
                {
                    OrderId = request.OrderId,
                    UserId = request.UserId,
                    Reason = "Falha ao finalizar a Order e adicionar os Games à Library do Usuário."
                });
            }
        }
    }
}
