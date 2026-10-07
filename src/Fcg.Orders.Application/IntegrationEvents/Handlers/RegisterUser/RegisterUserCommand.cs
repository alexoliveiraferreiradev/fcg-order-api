using MediatR;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.RegisterUser
{
    public record RegisterUserCommand(Guid EventId, DateTime CreatedAt, Guid UserId, string Name, string Email) : IRequest;
}
