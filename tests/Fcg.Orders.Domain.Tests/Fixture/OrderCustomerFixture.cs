using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Tests.Fixture
{
    public class OrderCustomerFixture
    {
        private Guid _guidCustomer;
        private string _nomeCustomer;   
        private string _emailCustomer;  
        public OrderCustomerFixture()
        {
            _guidCustomer = Guid.NewGuid();
            _nomeCustomer = "Qualquer Nome";
            _emailCustomer = "Qualquer Email";
        }

        public OrderCustomer GetSucessCustomer()
        {
            return new OrderCustomer(_guidCustomer, _nomeCustomer, _emailCustomer);
        }

        public OrderCustomer GetMissingCustomer()
        {
            return null;
        }
    }
}
