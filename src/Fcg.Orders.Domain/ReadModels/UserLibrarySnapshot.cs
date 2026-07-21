namespace Fcg.Orders.Domain.ReadModels
{
    public class UserLibrarySnapshot
    {
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public DateTime AcquiredAt { get; private set; }
        public DateTime LastSyncedAt { get; private set; }
        public UserLibrarySnapshot() { }    

        public void Create(Guid userId, Guid gameId,  DateTime acquiredAt, DateTime lastSyncedAt)
        {
            UserId = userId;    
            GameId = gameId;    
            AcquiredAt = acquiredAt;    
            LastSyncedAt = lastSyncedAt;    
        }
    }
}
