using Fcg.Orders.Domain.Entities;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.CreatedGame
{
    public class GameCreatedCommandHandler : IRequestHandler<GameCreatedCommand>
    {
        private readonly IGameSnapshotRepository _snapshotRepository;
        private readonly ILogger<GameCreatedCommandHandler> _logger;

        public GameCreatedCommandHandler(IGameSnapshotRepository snapshotRepository,
            ILogger<GameCreatedCommandHandler> logger)
        {
            _snapshotRepository = snapshotRepository;
            _logger = logger;
        }

        public async Task Handle(GameCreatedCommand request, CancellationToken cancellationToken)
        {
            var game = await _snapshotRepository.GetByIdAsync(request.GameId,cancellationToken);

            if (game is null)
            {
                game = new GameSnapshot(request.GameId, request.Name, request.Description,
                    request.Genre, request.Price, request.OccurredAt);
               
                var guidGame = await _snapshotRepository.AddSnapshot(game);

                _logger.LogInformation(
                    "[OrderAPI] Snapshot criado para o Jogo {GameId}", guidGame);
            }
            else
            {
                game.Update(request.Name, request.Description, request.Price,
                    request.IsAvailable, request.OccurredAt);

                var sucess = await _snapshotRepository.UpdateSnapshot(game);    
                
                if(sucess)
                {
                    _logger.LogInformation(
                        "[OrderAPI] Snapshot atualizado com sucesso para o Jogo {GameId}", request.GameId);
                }
                else
                {
                    _logger.LogError(
                        "[OrderAPI] Falha ao atualizar o snapshot para o Jogo {GameId}", request.GameId);
                }
            }

            
        }
    }
}
