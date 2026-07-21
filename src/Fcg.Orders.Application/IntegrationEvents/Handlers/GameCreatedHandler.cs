using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Application.Interfaces;
using Fcg.Orders.Application.ReadModels;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers
{
    public class GameCreatedHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameSnapshotRepository _snapshotRepository;

        public GameCreatedHandler(IUnitOfWork unitOfWork, IGameSnapshotRepository snapshotRepository)
        {
            _unitOfWork = unitOfWork;
            _snapshotRepository = snapshotRepository;
        }

        public async Task Handle(Guid gameId, string name, string description, decimal price, bool isAvaiable,
            DateTime occurredAt)
        {
            var game = await _snapshotRepository.GetSnapshotByIdAsync(gameId);

            if(game is null)
            {
                game = new GameSnapshot();
                game.Create(gameId, name, description, price);
            }
            else
            {
                game.Update(name, description, price,isAvaiable,occurredAt);
            }

           await _unitOfWork.CommitAsync();
        }
    }
}
