using Fcg.Core.SharedContracts.Interfaces;
using Fcg.Core.WebApi.MessageBroker;
using Fcg.Orders.Application.Common.Interfaces;
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

            services.AddOptions<OrderServiceRabbitMqSettings>().BindConfiguration(RabbitMqConnectionSettings.SectionName)
            .ValidateDataAnnotations().ValidateOnStart();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(GameCreatedConsumer).Assembly);
                x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqConfig = context.GetRequiredService<IOptions<OrderServiceRabbitMqSettings>>().Value;

                    cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, "/", h =>
                    {
                        h.Username(rabbitMqConfig.Username);
                        h.Password(rabbitMqConfig.Password);
                    });

                    cfg.UseConsumeFilter(typeof(LoggingConsumeFilter<>), context);

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)).Ignore<FluentValidation.ValidationException>());

                    cfg.ReceiveEndpoint(rabbitMqConfig.GameCreatedIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.GameDeactiveIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameDeactiveConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.RegisterUserIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<RegisteredUserConsumer>(context);   
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.GameReactiveIntegrationQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<GameReactiveConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.OrderPaymentFailedQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(rabbitMqConfig.OrderPaymentSucceededQueue, e =>
                    {
                        e.UseEntityFrameworkOutbox<OrderDbContext>(context);
                        e.ConfigureConsumer<PaymentProcessedEventConsumer>(context);
                    });

                });
            });

            return services;
        }
    }
}
