using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class GameSnapshotRepository : IGameSnapshotRepository
    {
        private readonly OrderDbContext _dbContext;

        public GameSnapshotRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddSnapShot(GameSnapshot snapShot)
        {
            _dbContext.GameSnapshots.Add(snapShot);
        }

        public void DeleteSnapShot(GameSnapshot snapShot)
        {
            _dbContext.GameSnapshots.Remove(snapShot);
        }

        public async Task<IEnumerable<GameSnapshot>> GetAllSnapshotsByIdsAsync(IEnumerable<Guid> gamesIds)
        {
            return await _dbContext.GameSnapshots.Where(x => gamesIds.Contains(x.GameId)).ToListAsync();
        }

        public async Task<GameSnapshot> GetSnapshotByIdAsync(Guid gameId)
        {
            return await _dbContext.GameSnapshots.FirstOrDefaultAsync(x => x.GameId == gameId);
        }

        public void UpdateSnapShot(GameSnapshot snapShot)
        {
           _dbContext.GameSnapshots.Update(snapShot);
        }
    }
}
