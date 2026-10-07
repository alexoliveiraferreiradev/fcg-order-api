using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Common.Interfaces;
using Fcg.Core.Abstractions.Resources;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.ValueObject;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Xml.Linq;

namespace Fcg.Orders.Domain.Entities
{
    public class Order :IAggregateRoot
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; private set; }
        public OrderCustomer Customer { get; private set; }
        public OrderStatus Status { get; private set; }
        public Price TotalAmount { get; private set; } = new Price(0);
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public IReadOnlyCollection<OrderGame> Games { get; private set; } = new List<OrderGame>();

        public Order(OrderCustomer customer)
        {
            Id = Guid.NewGuid();
            Customer = customer;
            Games = new List<OrderGame>();
            Status = OrderStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            ValidateEntity();
        }

        private void ValidateEntity()
        {
            if (Customer is null)
                throw new DomainException("Pedido precisa de um cliente associado.");
        }

        public void AddItem(Guid gameId, string gameName, decimal gameAmount)
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderGameNotDraft);
            if (gameId == Guid.Empty) throw new DomainException(DomainMessages.GameNotFound);
            if (Games.Any(j => j.GameId == gameId)) throw new DomainException(DomainMessages.OrderGameAlreadyAdded);

            ((List<OrderGame>)Games).Add(new OrderGame(gameId, gameName, gameAmount));
        }

        public void CancelOrder()
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderNotDraft);
            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MakeOrder()
        {
            CalculateTotalAmount();
            Status = OrderStatus.Draft;
            UpdatedAt = DateTime.UtcNow;
        }

        public void FinalizeOrder()
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderNotDraft);
            if (!Games.Any()) throw new DomainException(DomainMessages.OrderWithoutGames);
            Status = OrderStatus.Completed;
            CalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = new Price(Games.Sum(j => j.GameAmount));
        }
    }
}