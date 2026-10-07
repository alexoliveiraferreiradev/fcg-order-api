using Fcg.Core.SharedContracts.MessageContracts;
using Fcg.Orders.Application.IntegrationEvents.Handlers.RegisterUser;
using MassTransit;
using MediatR;

namespace Fcg.Orders.Infrastructure.Consumers
{
    public class RegisteredUserConsumer : IConsumer<IUserCreatedIntegrationEvent>
    {
        private readonly ISender _sender;

        public RegisteredUserConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<IUserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            var registerUserCommand = new RegisterUserCommand(message.EventId, message.CreatedAt, 
                message.UserId, message.Name, message.Email);
            
            await _sender.Send(registerUserCommand);
        }
    }
}
