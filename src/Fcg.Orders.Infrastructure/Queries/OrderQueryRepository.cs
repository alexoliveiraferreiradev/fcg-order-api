using Dapper;
using Fcg.Core.Abstractions.Common;
using Fcg.Orders.Application.Features.Response;
using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Infrastructure.Queries
{
    public class OrderQueryRepository : IOrderQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public OrderQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PagedResult<OrderHistoryResponse>> GetOrderHistoryByUserId(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var offset = (page-1) * pageSize;
            const string countSql = "SELECT COUNT(1) FROM Orders WHERE UserId = @UserId;";
            var totalItems = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { UserId = userId });

            if (totalItems == 0)
            {
                return new PagedResult<OrderHistoryResponse>(Enumerable.Empty<OrderHistoryResponse>(), page, pageSize, 0);
            }

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if(page > totalPages)
            {
                return new PagedResult<OrderHistoryResponse>(Enumerable.Empty<OrderHistoryResponse>(), page, pageSize, totalItems);
            }

            const string ordersSql = @"
                        SELECT o.Id
                        FROM Orders o    
                        where o.UserId = @UserId
                        ORDER By o.CreatedAt, o.Status DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";


            var orders = (await _dbConnection.QueryAsync<OrderHistoryResponse>(ordersSql, new { UserId = userId, Offset = offset, PageSize = pageSize })).ToList();

            var ordersIds = orders.Select(x => x.OrderId).ToList();

            const string itemsSql = @"
                         SELECT 
                         og.OrderId,
                         og.GameId,
                         og.GameName,
                         og.GameAmount as PaidPrice,
                         g.CurrentPrice as OriginalPrice
                         FROM OrdersGames og 
                         INNER JOIN GameSnapshots g 
                         on og.GameId = g.Id
                         WHERE og.OrderIn in @OrderIds;";

            var items = await _dbConnection.QueryAsync<dynamic>(itemsSql, new { OrderIds = ordersIds });

            var itemsGrouped = items.GroupBy(i => (Guid)i.OrderId).ToDictionary(g => g.Key, g => g.Select(i => new OrderItemResponse
            {
                GameId = i.GameId,
                GameName = i.GameName,
                PaidPrice = i.PaidPrice,
                OriginalPrice = i.OriginalPrice
            }).ToList());

            foreach (var order in orders)
            {
                if (itemsGrouped.TryGetValue(order.OrderId, out var orderItems))
                {
                    order.Items = orderItems;
                }
            }

            return new PagedResult<OrderHistoryResponse>(orders, page, pageSize, totalItems);
        }
    }
}
