using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Interfaces
{
    public interface IOrderQueryRepository
    {
        Task<PagedResult<OrderHistoryResponse>> GetOrderHistoryByUserId(Guid userId, int page =1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
