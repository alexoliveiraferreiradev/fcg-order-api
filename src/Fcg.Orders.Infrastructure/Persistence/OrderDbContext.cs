using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }    
        public DbSet<GameSnapshot> GameSnapshots { get; set; }  
        public DbSet<UserLibrarySnapshot> UserLibrarySnapshots { get; set; }    
        public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
