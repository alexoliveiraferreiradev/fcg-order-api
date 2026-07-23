using System.Globalization;

namespace Fcg.Orders.Application.ReadModels
{
    public class GameSnapshot
    {
        public Guid GameId { get;private  set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Genre { get; private set; } = string.Empty;
        public decimal CurrentPrice { get; private set; }
        public DateTime LastSycendAt { get; private set; }
        public bool IsAvailableForPurchase { get; private set; }

        public void ApplyPriceChange(decimal newPrice, DateTime occuredAt)
        {
            if (occuredAt < LastSycendAt) return;
            CurrentPrice = newPrice;
            LastSycendAt = occuredAt;
        }

        public void Create( Guid gameId, string nameGame, string descriptionGame, string genreGame, decimal currentPrice)
        {
            GameId = gameId;
            Name = nameGame;
            Genre = genreGame;
            Description = descriptionGame;
            CurrentPrice = currentPrice;
            LastSycendAt = DateTime.UtcNow;
            IsAvailableForPurchase = true;
        }

        public void Update(string nameGame, string descriptionGame, decimal currentPrice, bool isAvaiable, DateTime occurredAt)
        {
            Name = nameGame;
            Description = descriptionGame;
            CurrentPrice = currentPrice;
            IsAvailableForPurchase = isAvaiable;    
            LastSycendAt = occurredAt;  
        }

    }
}
