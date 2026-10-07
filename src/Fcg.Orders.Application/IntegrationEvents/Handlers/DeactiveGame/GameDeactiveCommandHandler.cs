using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.DeactiveGame
{
    public class GameDeactiveCommandHandler : IRequestHandler<GameDeactiveCommand>
    {
        private readonly IGameSnapshotRepository _snapshotRepository;
        private readonly ILogger<GameDeactiveCommandHandler> _logger;

        public GameDeactiveCommandHandler( IGameSnapshotRepository snapshotRepository,
            ILogger<GameDeactiveCommandHandler> logger)
        {
            _snapshotRepository = snapshotRepository;
            _logger = logger;
        }


        public async Task Handle(GameDeactiveCommand request, CancellationToken cancellationToken)
        {
            var game = await _snapshotRepository.GetByIdAsync(request.GameId, cancellationToken);

            if (game is null)
            {
                _logger.LogWarning("[OrderAPI] Evento de desativação ignorado: snapshot inexistente para o Jogo {GameId}", request.GameId);
                return;
            }

            game.Deactivate(request.OccurredAt);
            var sucess = await _snapshotRepository.UpdateSnapshot(game);

            if(sucess)
            {
                _logger.LogInformation("[OrderAPI] Snapshot do Jogo {GameId} desativado com sucesso", game.GameId);
            }
            else
            {
                _logger.LogWarning("[OrderAPI] Falha ao desativar o snapshot do Jogo {GameId}", game.GameId);
            }
        }   
    }
}
