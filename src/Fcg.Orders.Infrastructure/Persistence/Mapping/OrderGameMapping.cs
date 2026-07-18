using Fcg.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Orders.Infrastructure.Persistence.Mapping
{
    public class OrderGameMapping : IEntityTypeConfiguration<OrderGame>
    {
        public void Configure(EntityTypeBuilder<OrderGame> builder)
        {
            builder.ToTable("OrdersGame");

            builder.HasKey(og => og.Id);

            builder.Property(og => og.OrderId)
                .IsRequired();

            builder.Property(og => og.GameId)
                .IsRequired();

            builder.Property(og => og.GameName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(og => og.GameAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        }
    }
}
