using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fcg.Orders.Infrastructure.Consumers.Filters
{
    /// <summary>
    /// Aplica um escopo de log com os identificadores da mensagem a todos os consumers,
    /// permitindo correlacionar o log do consumer com o log dos handlers da Application.
    /// </summary>
    public class LoggingConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly ILogger<LoggingConsumeFilter<T>> _logger;

        public LoggingConsumeFilter(ILogger<LoggingConsumeFilter<T>> logger)
        {
            _logger = logger;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var messageType = typeof(T).Name;

            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["MessageId"] = context.MessageId,
                ["CorrelationId"] = context.CorrelationId,
                ["MessageType"] = messageType
            });

            _logger.LogInformation("[OrderAPI] Mensagem {MessageType} recebida", messageType);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next.Send(context);

                stopwatch.Stop();
                _logger.LogInformation(
                    "[OrderAPI] Mensagem {MessageType} processada em {ElapsedMilliseconds}ms",
                    messageType, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "[OrderAPI] Falha ao processar a mensagem {MessageType} após {ElapsedMilliseconds}ms. Tentativa {RetryAttempt}.",
                    messageType, stopwatch.ElapsedMilliseconds, context.GetRetryAttempt());
                throw;
            }
        }

        public void Probe(ProbeContext context) => context.CreateFilterScope("logging");
    }
}
