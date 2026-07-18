using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Application.ReadModels
{
    public class UserLibrarySnapshot
    {
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public DateTime AcquiredAt { get; private set; }
        public DateTime LastSyncedAt { get; private set; }

        public void Criar(Guid userId, Guid gameId, DateTime acquiredAt, DateTime occuredAt)
        {
            UserId = userId;
            GameId = gameId;    
            AcquiredAt = acquiredAt;    
            LastSyncedAt = occuredAt;
        }
    }
}
