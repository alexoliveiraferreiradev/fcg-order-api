using Fcg.Core.Abstractions.Interfaces;

namespace Fcg.Orders.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _dbContext;

        public UnitOfWork(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync () > 0;    
        }
    }
}
