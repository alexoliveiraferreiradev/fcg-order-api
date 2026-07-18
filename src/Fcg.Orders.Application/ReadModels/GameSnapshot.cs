namespace Fcg.Orders.Application.ReadModels
{
    public class GameSnapshot
    {
        public Guid GameId { get;private  set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal CurrentPrice { get; private set; }
        public DateTime LastSycendAt { get; private set; }
        public bool IsAvailableForPurchase { get; private set; }

        public void ApplyPriceChange(decimal newPrice, DateTime occuredAt)
        {
            if (occuredAt < LastSycendAt) return;
            CurrentPrice = newPrice;
            LastSycendAt = occuredAt;
        }

        public void Create( Guid gameId, string nameGame, string descriptionGame, decimal currentPrice)
        {
            GameId = gameId;
            Name = nameGame;
            Description = descriptionGame;
            CurrentPrice = currentPrice;
            LastSycendAt = DateTime.UtcNow;
            IsAvailableForPurchase = true;
        }

        public void Deactive(Guid gameId, string nameGame, string descriptionGame, decimal currentPrice)
        {
            GameId = gameId;
            Name = nameGame;
            Description = descriptionGame;
            CurrentPrice = currentPrice;
            LastSycendAt = DateTime.UtcNow;
            IsAvailableForPurchase = false;
        }
    }
}
