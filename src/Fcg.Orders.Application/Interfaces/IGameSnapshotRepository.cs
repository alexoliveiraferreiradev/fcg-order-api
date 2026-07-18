using Fcg.Orders.Application.ReadModels;

namespace Fcg.Orders.Application.Interfaces
{
    public interface IGameSnapshotRepository
    {
        Task<GameSnapshot> GetSnapshotByIdAsync(Guid gameId);
        Task<IEnumerable<GameSnapshot>> GetAllSnapshotsByIdsAsync(IEnumerable<Guid> gamesIds);
        void AddSnapShot(GameSnapshot snapShot);
        void UpdateSnapShot(GameSnapshot snapShot);
        void DeleteSnapShot(GameSnapshot snapShot);
    }
}
