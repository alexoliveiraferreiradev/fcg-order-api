using Fcg.Core.Abstractions.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fcg.Orders.Application.Common.Behaviors
{
    /// <summary>
    /// Registra início, conclusão e falha de todo request que passa pelo MediatR,
    /// independente da origem (endpoint HTTP ou consumer de mensageria).
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation("[OrderAPI] Iniciando {RequestName}", requestName);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();
                _logger.LogInformation(
                    "[OrderAPI] {RequestName} concluído em {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (DomainException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "[OrderAPI] {RequestName} rejeitado por regra de negócio após {ElapsedMilliseconds}ms: {Motivo}",
                    requestName, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[OrderAPI] {RequestName} falhou após {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
