using Dapper;
using Fcg.Orders.Domain.ValueObject;
using System.Data;

namespace Fcg.Orders.Infrastructure.Queries.DapperHandler
{
    public class PriceTypeHandler : SqlMapper.TypeHandler<Price>
    {
        public override void SetValue(IDbDataParameter parameter, Price? value)
        {
            parameter.Value = value?.Amount ?? (object)DBNull.Value;
        }

        public override Price Parse(object value)
        {
            var valorStr = Convert.ToDecimal(value?.ToString() ?? "0");
            return new Price(valorStr);
        }
    }
}
