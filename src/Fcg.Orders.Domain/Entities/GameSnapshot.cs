using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Resources;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.Orders.Domain.Entities
{
    public class GameSnapshot
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;
        public decimal CurrentPrice { get; private set; }
        public DateTime LastSyncedAt { get; private set; }
        public bool IsAvailableForPurchase { get; private set; }

        public void ApplyPriceChange(decimal newPrice, DateTime occuredAt)
        {
            if (occuredAt < LastSyncedAt) return;
            CurrentPrice = newPrice;
            LastSyncedAt = occuredAt;
        }

        protected GameSnapshot()
        {
        }

        public GameSnapshot(Guid gameId, string name, string description, string genre, decimal currentPrice,
            DateTime occurredAt)
        {
            GameId = gameId;
            Name = name;
            Genre = genre;
            Description = description;
            CurrentPrice = currentPrice;
            LastSyncedAt = occurredAt;
            IsAvailableForPurchase = true;
            ValidateEntity();
        }

        public void Update(string nameGame, string descriptionGame, decimal currentPrice, bool isAvaiable, DateTime occurredAt)
        {
            if(occurredAt < LastSyncedAt) return;
            Name = nameGame;
            Description = descriptionGame;
            CurrentPrice = currentPrice;
            IsAvailableForPurchase = isAvaiable;
            LastSyncedAt = occurredAt;
        }

        public void Deactivate(DateTime occurredAt)
        {
            if (occurredAt < LastSyncedAt) return;
            IsAvailableForPurchase = false;
            LastSyncedAt = occurredAt;
        }

        public void Reactivate(DateTime occurredAt)
        {
            if (occurredAt < LastSyncedAt) return;
            IsAvailableForPurchase = true;
            LastSyncedAt = occurredAt;
        }

        private void ValidateEntity()
        {
            AssertionConcern.AssertArgumentEmpty(Name,DomainMessages.GameNameRequired);
            AssertionConcern.AssertArgumentEmpty(Description,DomainMessages.GameDescriptionRequired);
            AssertionConcern.AssertArgumentLength(Description, 5, 500, DomainMessages.GameDescriptionLengthInvalid);
            AssertionConcern.AssertArgumentLength(Name, 3, 100, DomainMessages.GameNameLengthInvalid);
        }
    }
}
