using Fcg.Orders.Application.Extension;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder
{
    public class FinalizeFailedOrderCommandHandler : IRequestHandler<FinalizeFailedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<FinalizeFailedOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        public FinalizeFailedOrderCommandHandler(IOrderRepository orderRepository, 
             ILogger<FinalizeFailedOrderCommandHandler> logger, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(FinalizeFailedCommand request, CancellationToken cancellationToken)
        {
            var orderUser = await _orderRepository.GetOrderById(request.OrderId,cancellationToken);

            orderUser.CancelOrder();
                      
            var sucess = await _orderRepository.UpdateOrder(orderUser);
                      

            if(sucess)
            {
                _logger.LogInformation("[OrderAPI] Pedido cancelado com sucesso para o Usuário: {UserId}", orderUser.Customer.CustomerId);
                await _mediator.Publish(new OrderEvent(orderUser.Customer.CustomerId));
                _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", orderUser.Customer.CustomerId);
            }
            else
            {
                _logger.LogWarning("[OrderAPI] Falha ao cancelar o pedido para o Usuário: {UserId}", orderUser.Customer.CustomerId);
            }
                        
        }
    }
}
