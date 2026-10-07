using Fcg.Core.Abstractions.Common;
using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.Orders.Domain.Entities
{
    public class OrderGame
    {
        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; private set; }
        public string GameName { get; private set; }
        public decimal GameAmount { get; private set; }

        private OrderGame()
        {
        }

        public OrderGame( Guid gameId, string gameName, decimal gameAmount)
        {
            GameId = gameId;
            GameName = gameName;
            GameAmount = gameAmount;
            ValidateEntity();
        }

        private void ValidateEntity()
        {
            AssertionConcern.AssertArgumentNotEmpty(GameId, DomainMessages.GameNotFound);
            AssertionConcern.AssertArgumentNotNull(GameName, DomainMessages.GameNameRequired);

            if (GameAmount < 0)
                throw new DomainException("O Valor do item não pode ser negativo");
        }
    }
}
