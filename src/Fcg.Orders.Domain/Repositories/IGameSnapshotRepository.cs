using Fcg.Orders.Domain.Entities;

namespace Fcg.Orders.Domain.Repositories
{
    public interface IGameSnapshotRepository
    {
        Task<GameSnapshot> GetByIdAsync(Guid gameId,CancellationToken cancellationToken);
        Task<IEnumerable<GameSnapshot>> GetAllByIdsAsync(IEnumerable<Guid> gamesIds,CancellationToken cancellation);
        Task<Guid> AddSnapshot(GameSnapshot snapShot);
        Task<bool> UpdateSnapshot(GameSnapshot snapShot);
        Task<bool> DeleteSnapshot(GameSnapshot snapShot);
    }
}
