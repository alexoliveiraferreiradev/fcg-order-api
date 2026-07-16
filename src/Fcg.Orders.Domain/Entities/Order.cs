using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.Orders.Domain.Enum;
using Fcg.Orders.Domain.ValueObject;

namespace Fcg.Orders.Domain.Entities
{
    public class Order : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public OrderStatus Status { get; private set; }
        public Price TotalAmount { get; private set; } = new Price(0);
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        private List<OrderGame> _games;
        public IReadOnlyCollection<OrderGame> Games => _games;

        protected Order()
        {
        }

        public Order(Guid userId)
        {
            UserId = userId;
            _games = new List<OrderGame>();
            Status = OrderStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            ValidateEntity();
        }

        protected override void ValidateEntity()
        {
            if (UserId == Guid.Empty) throw new DomainException(DomainMessages.OrderWithoutUser);
        }

        public void AddItem(Guid gameId, string gameName, decimal gameAmount)
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderGameNotDraft);
            if (gameId == Guid.Empty) throw new DomainException(DomainMessages.GameNotFound);
            if (_games.Any(j => j.GameId == gameId)) throw new DomainException(DomainMessages.OrderGameAlreadyAdded);

            _games.Add(new OrderGame(Id, gameId, gameName, gameAmount));
        }

        public void MakeOrder()
        {
            CalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void CancelOrder()
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderNotDraft);
            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void FinalizeOrder()
        {
            if (Status != OrderStatus.Draft) throw new DomainException(DomainMessages.OrderNotDraft);
            if (!_games.Any()) throw new DomainException(DomainMessages.OrderWithoutGames);
            Status = OrderStatus.Completed;
            CalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = new Price(_games.Sum(j => j.GameAmount));
        }
    }
}