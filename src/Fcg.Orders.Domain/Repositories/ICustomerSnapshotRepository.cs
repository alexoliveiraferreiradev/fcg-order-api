using Fcg.Orders.Application.ReadModels;

namespace Fcg.Orders.Domain.Repositories
{
    public interface ICustomerSnapshotRepository
    {
        Task<Guid> AddCustomer(CustomerSnapshot customerSnapshot);
       
        Task<CustomerSnapshot> GetCustomerByIdAsync(Guid id,CancellationToken cancellation);
    }
}
