using Fcg.Core.Abstractions.Interfaces;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.ReactiveGame
{
    public class GameReactiveCommandHandler : IRequestHandler<GameReactiveCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameSnapshotRepository _gameSnapshotRepository;
        private readonly ILogger<GameReactiveCommandHandler> _logger;

        public GameReactiveCommandHandler(IGameSnapshotRepository gameSnapshotRepository, 
            ILogger<GameReactiveCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _gameSnapshotRepository = gameSnapshotRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(GameReactiveCommand request, CancellationToken cancellationToken)
        {
            var game = await _gameSnapshotRepository.GetByIdAsync(request.GameId, cancellationToken);
            if (game is null)
            {
                _logger.LogWarning("[OrderAPI] Evento de reativação ignorado: snapshot inexistente para o Jogo {GameId}", request.GameId);
                return;
            }
            game.Reactivate(request.OccurredAt);

            var sucess = await _gameSnapshotRepository.UpdateSnapshot(game);
            if(sucess)
            {
                _logger.LogInformation(
                       "[OrderAPI] Snapshot reativado com sucesso para o jogo {GameId}", game.GameId);
            }
            else
            {
                _logger.LogWarning(
                    "[OrderAPI] Falha ao reativar o snapshot para o jogo {GameId}", game.GameId);
            }
        }
    }
}
