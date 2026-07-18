using Fcg.Orders.Application.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Application.Interfaces
{
    public interface IGameSnapshot
    {
        Task<GameSnapshot> GetSnapshotByIdAsync(Guid gameId);
        void AddSnapShot(GameSnapshot snapShot);
        void UpdateSnapShot(GameSnapshot snapShot);
        void DeleteSnapShot(GameSnapshot snapShot);
    }
}
