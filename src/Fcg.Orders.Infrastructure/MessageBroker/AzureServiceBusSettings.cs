using System.ComponentModel.DataAnnotations;

namespace Fcg.Orders.Infrastructure.MessageBroker
{
    public class AzureServiceBusSettings
    {
        public const string SectionName = "AzureServiceBus";
        [Required]
        public string ConnectionString { get; init; } = string.Empty;
        [Required(ErrorMessage = "A fila de compra de pedido falhou é obrigatória.")]
        public string OrderPurchasedFailedQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "A fila de compra de pedido bem-sucedido é obrigatória.")]
        public string OrderPurchasedSucceededQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "A fila de integração de criação de jogo é obrigatória.")]
        public string GameCreatedIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "A fila de integração de atualização de jogo é obrigatória.")]
        public string GameReactiveIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "A fila de integração de desativação de jogo é obrigatória.")]
        public string GameDeactiveIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "A fila de cadastro do usuário é obrigatória.")]
        public string RegisterUserIntegrationQueue { get; set; } = string.Empty;
    }
}
