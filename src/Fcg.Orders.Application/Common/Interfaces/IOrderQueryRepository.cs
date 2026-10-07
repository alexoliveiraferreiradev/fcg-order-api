using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Application.Common.Interfaces
{
    public interface IOrderQueryRepository
    {
        Task<PagedResult<Order>> GetOrderHistoryByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    }
}
