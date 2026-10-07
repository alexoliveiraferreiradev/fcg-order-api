using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Repositories;
using MongoDB.Driver;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class GameSnapshotRepository : IGameSnapshotRepository
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<GameSnapshot> _collection;

        public GameSnapshotRepository( IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<GameSnapshot>("games");   
        }

        public async Task<Guid> AddSnapshot(GameSnapshot snapShot)
        {
            await _collection.InsertOneAsync(snapShot);
            return snapShot.GameId;
        }

        public async Task<bool> DeleteSnapshot(GameSnapshot snapShot)
        {
            var result = await _collection.DeleteOneAsync(x => x.GameId == snapShot.GameId);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<GameSnapshot>> GetAllByIdsAsync(IEnumerable<Guid> gamesIds, CancellationToken cancellationToken)
        {
            var filter = Builders<GameSnapshot>.Filter.In(x => x.GameId, gamesIds);
            return await _collection.Find(filter).ToListAsync(cancellationToken);  
        }

        public async Task<GameSnapshot> GetByIdAsync(Guid gameId, CancellationToken cancellationToken)
        {
            var filter = Builders<GameSnapshot>.Filter.Eq(x => x.GameId, gameId);
            return await _collection.Find(filter).SingleOrDefaultAsync(cancellationToken);   
        }

        public async Task<bool> UpdateSnapshot(GameSnapshot snapShot)
        {
            var result = await _collection.ReplaceOneAsync(x => x.GameId == snapShot.GameId, snapShot);
            return result.ModifiedCount > 0;
        }
    }
}
