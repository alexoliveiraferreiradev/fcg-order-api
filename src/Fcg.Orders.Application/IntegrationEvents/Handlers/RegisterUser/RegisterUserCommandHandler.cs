using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fcg.Orders.Application.IntegrationEvents.Handlers.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand>
    {
        private readonly ICustomerSnapshotRepository _customerSnapshot;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        public RegisterUserCommandHandler(ICustomerSnapshotRepository customerSnapshot, ILogger<RegisterUserCommandHandler> logger)
        {
            _customerSnapshot = customerSnapshot;
            _logger = logger;
        }

        public async Task Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerSnapshot.GetCustomerByIdAsync(request.UserId, cancellationToken);
            if (customer is null)
            {
                var newCustomer = new CustomerSnapshot(request.UserId, request.Name, request.Email);
                await _customerSnapshot.AddCustomer(newCustomer);

                _logger.LogInformation(
                        "[OrderAPI] Usuário com {UserId} cadastrado com sucesso", request.UserId);
            }
            else
            {
                _logger.LogInformation(
                        "[OrderAPI] Usuário com {UserId} já cadastrado", request.UserId);
                return;
            }
        }
    }
}
