using Fcg.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Orders.Infrastructure.Persistence.Mapping
{
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.UserId)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(p => p.CreatedAt)
               .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .IsRequired();

            builder.OwnsOne(o => o.TotalAmount, Price =>
            {
                Price.Property(p => p.Amount)
                    .HasColumnName("TotalAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            builder.HasMany(o => o.Games)
                .WithOne()
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Order.Games))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);

        }
    }
}
