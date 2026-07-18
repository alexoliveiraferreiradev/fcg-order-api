using Fcg.Orders.Application.ReadModels;
using Fcg.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Orders.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }    
        public DbSet<GameSnapshot> GameSnapshots { get; set; }  
        public OrderDbContext(DbContextOptions<OrderDbContext> options): base(options)
        {            
        }
    }
}
