using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Features.Response;
using MediatR;

namespace Fcg.Orders.Application.Features.Queries.GetOrderHistory
{
    public record GetOrderHistoryQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<OrderHistoryResponse>>;
}
