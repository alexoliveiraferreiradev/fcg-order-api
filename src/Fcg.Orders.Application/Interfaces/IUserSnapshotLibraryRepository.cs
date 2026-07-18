using Fcg.Orders.Application.ReadModels;

namespace Fcg.Orders.Application.Interfaces
{
    public interface IUserSnapshotLibraryRepository
    {
        Task<IEnumerable<UserLibrarySnapshot>> GetPurchasedGamesByUserIdAsync(Guid userId,IEnumerable<Guid> gamesIds);
    }
}
