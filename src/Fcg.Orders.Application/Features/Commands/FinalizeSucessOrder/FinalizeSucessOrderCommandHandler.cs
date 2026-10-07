using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Extension;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.FinalizeSucessOrder
{
    public class FinalizeSucessOrderCommandHandler : IRequestHandler<FinalizeSucessOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<FinalizeSucessOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly ICustomerSnapshotRepository _userLibrarySnapshotRepository;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;
        private readonly ICustomerSnapshotRepository _customerSnapshotRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FinalizeSucessOrderCommandHandler(IOrderRepository orderRepository,
            ILogger<FinalizeSucessOrderCommandHandler> logger, IMediator mediator,
            ICustomerSnapshotRepository userLibrarySnapshotRepository,
            IGameSnapshotRepository gameSnapshotRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            ICustomerSnapshotRepository customerSnapshotRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _mediator = mediator;
            _userLibrarySnapshotRepository = userLibrarySnapshotRepository;
            _customerSnapshotRepository = customerSnapshotRepository;
            _gameSnapshotRepository = gameSnapshotRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(FinalizeSucessOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;
                var orderUser = await _orderRepository.GetOrderById(request.OrderId, cancellationToken);
                _logger.LogInformation("[OrderAPI] Pagamento aprovado para o Pedido: {OrderId}. Adicionando Jogos à Biblioteca do Usuário: {UserId}", request.OrderId, request.UserId);

                orderUser.FinalizeOrder();

                var sucess = await _orderRepository.UpdateOrder(orderUser);

                if (sucess)
                {
                    _logger.LogInformation("[OrderAPI] Pedido {OrderId} do Usuário {UserId} aprovado com sucesso.", orderUser.Id, orderUser.Customer.CustomerId);
                    await _mediator.Publish(new OrderEvent(orderUser.Customer.CustomerId));
                    _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", orderUser.Customer.CustomerId);
                }
                else
                {
                    _logger.LogWarning("[OrderAPI] Falha ao aprovar o Pedido {OrderId} do Usuário {UserId}.", orderUser.Id, orderUser.Customer.CustomerId);
                    return;
                }

                foreach (var guidJogo in request.GameIds)
                {
                    var gamesnapshot = await _gameSnapshotRepository.GetByIdAsync(guidJogo, cancellationToken);

                    var customer = await _customerSnapshotRepository.GetCustomerByIdAsync(orderUser.Customer.CustomerId, cancellationToken);


                    await _integrationEventPublisher.PublishAsync<IGameUserLibraryIntegrationEvent>(new
                    {
                        EventId = request.EventId,
                        CreatedAt = request.CreatedAt,
                        UserId = customer.CustomerId,
                        GameId = guidJogo,
                        Name = gamesnapshot.Name,
                        IsAvaiable = true,
                        Description = gamesnapshot.Description,
                        Genre = gamesnapshot.Genre,
                        OccurredAt = now
                    });

                }
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[OrderAPI] Falha ao aprovar o Pedido {OrderId} do Usuário {UserId}. Publicando DeliveryFailedEvent para compensação.",
                    request.OrderId, request.UserId);

                await _integrationEventPublisher.PublishAsync<IDeliveryFailedIntegrationEvent>(new
                {
                    EventId = request.EventId,
                    CreatedAt = request.CreatedAt,
                    OrderId = request.OrderId,
                    UserId = request.UserId,
                    Reason = "Falha ao aprovar a Order e adicionar os Games à Library do Usuário."
                });

                await _unitOfWork.CommitAsync();
            }
        }
    }
}
