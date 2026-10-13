using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Core.WebApi.MessageBroker;
using Fcg.Orders.Infrastructure.Consumers;
using Fcg.Orders.Infrastructure.Consumers.Filters;
using Fcg.Orders.Infrastructure.MessageBroker;
using Fcg.Orders.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fcg.Orders.Infrastructure.Extensions
{
    internal static class MessageBrokerExtension
    {
        public static IServiceCollection AddMassTransitExtension(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();

            services.AddOptions<AzureServiceBusSettings>().BindConfiguration(AzureServiceBusSettings.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(GameCreatedConsumer).Assembly);
                x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    var serviceBusConfig = context.GetRequiredService<IOptions<AzureServiceBusSettings>>().Value;
                    cfg.Host(serviceBusConfig.ConnectionString);

                    cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)).Ignore<FluentValidation.ValidationException>());

                    cfg.Message<IUserCreatedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.UserCreated));
                    cfg.Message<IDeliveryFailedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.DeliveryFailed));
                    cfg.Message<IPaymentApprovedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.PaymentApproved));
                    cfg.Message<IPurchaseRejectedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.PurchaseRejected));
                    cfg.Message<IGameCreatedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameCreated));
                    cfg.Message<IGameDeactiveIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameDeactivated));
                    cfg.Message<IGameReactiveIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameReactivated));
                    cfg.Message<IGameUserLibraryIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.GameUserLibrary));
                    cfg.Message<IOrderPlacedIntegrationEvent>(m => m.SetEntityName(ServiceBusTopics.OrderPlaced));

                    cfg.ReceiveEndpoint(serviceBusConfig.GameCreatedIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.GameDeactiveIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameDeactiveConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.RegisterUserIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<RegisteredUserConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.GameReactiveIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameReactiveConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.OrderPurchasedFailedQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PurchasedRejectEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(serviceBusConfig.OrderPurchasedSucceededQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PaymentApprovedEventConsumer>(context);
                    });

                });


               
            });

            return services;
        }
    }
}
