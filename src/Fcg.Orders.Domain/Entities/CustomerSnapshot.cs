using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.Orders.Application.ReadModels
{
    public class CustomerSnapshot
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid CustomerId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        private CustomerSnapshot() { }

        public CustomerSnapshot(Guid customerId,string name, string email)
        {
            CustomerId = customerId;
            Name = name;
            Email = email;
        }
    }
}
