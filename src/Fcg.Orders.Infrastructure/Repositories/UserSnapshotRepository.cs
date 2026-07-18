using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Repositories
{
    public class UserSnapshotRepository : IUserSnapshotLibraryRepository
    {
        private readonly OrderDbContext _dbContext;

        public UserSnapshotRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserLibrarySnapshot>> GetPurchasedGamesByUserIdAsync(Guid userId, IEnumerable<Guid> gamesIds)
        {            
            return await _dbContext.UserLibrarySnapshots.Where(x=>x.UserId == userId && gamesIds.Contains(x.UserId)).ToListAsync();
        }
    }
}
