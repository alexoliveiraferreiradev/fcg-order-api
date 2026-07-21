using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Domain.Interfaces;
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
            var cacheKey = $"order_u:{request.UserId}_p:{request.Page}_s:{request.PageSize}";
            var cachedOrderHistory = await _cacheService.GetAsync<PagedResult<OrderHistoryResponse>>(cacheKey, cancellationToken);

            if (cachedOrderHistory != null)
            {
                return cachedOrderHistory;
            }

            var pagedOrder = await _orderQueryRepository.GetOrderHistoryByUserId(request.UserId, request.Page, request.PageSize, cancellationToken);

            if (pagedOrder.Items.Any())
            {
                await _cacheService.SetAsync(cacheKey, pagedOrder, TimeSpan.FromMinutes(5), cancellationToken);
            }

            return pagedOrder;
        }
    }
}
