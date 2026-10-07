using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.Orders.Domain.Entities
{
    public class OrderCustomer
    {
        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        private OrderCustomer() { }

        public OrderCustomer(Guid customerId, string name, string email)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
        }
    }
}
