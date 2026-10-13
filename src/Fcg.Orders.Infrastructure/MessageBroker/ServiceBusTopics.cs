namespace Fcg.Orders.Infrastructure.MessageBroker
{
    /// <summary>
    /// Nomes fixos dos tópicos no Azure Service Bus (via SetEntityName).
    /// Precisam ser iguais em todos os serviços que publicam ou consomem o evento.
    /// </summary>
    internal static class ServiceBusTopics
    {
        public const string UserCreated = "user-created";
        public const string UserRepublished = "user-republished";
        public const string UserDeactivated = "user-deactivated";
        public const string UserReactivated = "user-reactivated";
        public const string GameCreated = "game-created";
        public const string GameDeactivated = "game-deactivated";
        public const string GameReactivated = "game-reactivated";
        public const string GameUserLibrary = "game-user-library";
        public const string OrderPlaced = "order-placed";
        public const string PaymentApproved = "payment-approved";
        public const string PurchaseRejected = "purchase-rejected";
        public const string DeliveryFailed = "delivery-failed";
    }
}
