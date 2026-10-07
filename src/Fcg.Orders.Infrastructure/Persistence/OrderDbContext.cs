using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {      
        public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
