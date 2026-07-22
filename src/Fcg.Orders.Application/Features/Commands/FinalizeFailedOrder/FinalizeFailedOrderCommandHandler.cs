using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Domain.Events;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.Features.Commands.FinalizeFailedOrder
{
    public class FinalizeFailedOrderCommandHandler : IRequestHandler<FinalizeFailedCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinalizeFailedOrderCommandHandler> _logger;
        private readonly IMediator _mediator;
        public FinalizeFailedOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<FinalizeFailedOrderCommandHandler> logger, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(FinalizeFailedCommand request, CancellationToken cancellationToken)
        {
            var orderUser = await _orderRepository.GetOrderById(request.OrderId);

            orderUser.CancelOrder();

            _orderRepository.UpdateOrder(orderUser);

            _logger.LogInformation("[OrderAPI] Publicado OrderEvent para o Usuário: {UserId}", orderUser.UserId);

            await _mediator.Publish(new OrderEvent(orderUser.UserId));

            await _unitOfWork.CommitAsync();
        }
    }
}
