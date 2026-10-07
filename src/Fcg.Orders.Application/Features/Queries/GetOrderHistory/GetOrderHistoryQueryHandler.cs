using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Common.Interfaces;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Domain.Enum;
using MediatR;

namespace Fcg.Orders.Application.Features.Queries.GetOrderHistory
{
    public class GetOrderHistoryQueryHandler : IRequestHandler<GetOrderHistoryQuery, PagedResult<OrderHistoryResponse>>
    {
        private readonly ICacheService _cacheService;
        private readonly IOrderQueryRepository _orderQueryRepository;

        public GetOrderHistoryQueryHandler(IOrderQueryRepository orderQueryRepository, ICacheService cacheService)
        {
            _orderQueryRepository = orderQueryRepository;
            _cacheService = cacheService;
        }
        public async Task<PagedResult<OrderHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"order:_u:{request.UserId}:_p:{request.Page}:_s:{request.PageSize}";
            var cachedOrderHistory = await _cacheService.GetAsync<PagedResult<OrderHistoryResponse>>(cacheKey, cancellationToken);

            if (cachedOrderHistory != null)
            {
                return cachedOrderHistory;
            }

            var pagedOrder = await _orderQueryRepository.GetOrderHistoryByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);

            var response = pagedOrder.Items.Select(o => new OrderHistoryResponse
            {
                OrderId = o.Id,
                OrderDate = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount.Amount,
                Items = o.Games.Select(g=> new OrderItemResponse
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    PaidPrice = o.Status == OrderStatus.Completed ? g.GameAmount : 0
                }).ToList()
            }).ToList();

            var result = new PagedResult<OrderHistoryResponse>(response, request.Page, request.PageSize, pagedOrder.TotalItems);

            if (pagedOrder.Items.Any())
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
            }

            return result;
        }
    }
}
