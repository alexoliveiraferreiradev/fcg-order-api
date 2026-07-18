using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Infrastructure.Queries
{
    public class OrderQueryRepository : IOrderQueryRepository
    {

        public Task<IEnumerable<Order>> GetOrderHistoryByUserId(Guid userId)
        {
            
            throw new NotImplementedException();
        }
    }
}
