using Fcg.Core.Abstractions.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Application.ReadModels;
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

        public FinalizeSucessOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<FinalizeSucessOrderCommandHandler> logger, IMediator mediator, IPublishEndpoint publishEndpoint, IUserSnapshotLibraryRepository userLibrarySnapshotRepository)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
            _publishEndpoint = publishEndpoint;
            _userLibrarySnapshotRepository = userLibrarySnapshotRepository;
        }

        public async Task Handle(FinalizeSucessOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                
                var orderUser = await _orderRepository.GetOrderById(request.OrderId);
                _logger.LogInformation("[CatalogAPI] Pagamento aprovado para o Pedido: {OrderId}. Adicionando Jogos à Biblioteca do Usuário: {UserId}", request.OrderId, request.UserId);

                foreach (var guidJogo in request.GameIds)
                {
                    var librarySnapshot = new UserLibrarySnapshot(orderUser.UserId,guidJogo);
                    _userLibrarySnapshotRepository.AddUserLibrary(librarySnapshot); 

                    

                }
                //await _mediator.Publish(new LibraryEvent(request.UserId));

                orderUser.FinalizeOrder();

                _logger.LogInformation("[CatalogAPI] Publicado LibraryEvent para o Usuário: {UserId}", request.UserId);


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
