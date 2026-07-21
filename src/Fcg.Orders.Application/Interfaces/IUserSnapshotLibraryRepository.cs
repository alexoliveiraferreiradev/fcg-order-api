using Fcg.Orders.Application.ReadModels;

namespace Fcg.Orders.Application.Interfaces
{
    public interface IUserSnapshotLibraryRepository
    {
        void AddUserLibrary(UserLibrarySnapshot librarySnapshot);
        Task<IEnumerable<UserLibrarySnapshot>> GetPurchasedGamesByUserIdAsync(Guid userId,IEnumerable<Guid> gamesIds);
    }
}
