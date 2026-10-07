using Prometheus;

namespace Fcg.Orders.Application.Extension
{
    internal static class OrderMetrics
    {
        internal static readonly Counter OrderAttempts = Metrics.CreateCounter(
            "order_attempts_total",
            "Número total de tentativas de realizar pedido.",
            new CounterConfiguration { LabelNames = new[] { "result" } });
    }
}
