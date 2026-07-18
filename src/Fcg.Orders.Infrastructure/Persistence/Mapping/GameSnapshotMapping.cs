using Fcg.Orders.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Infrastructure.Persistence.Mapping
{
    public class GameSnapshotMapping : IEntityTypeConfiguration<GameSnapshot>
    {
        public void Configure(EntityTypeBuilder<GameSnapshot> builder)
        {
            builder.ToTable("GameSnapshots");

            builder.HasKey(g => g.GameId);

            builder.Property(g => g.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(g => g.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(g => g.CurrentPrice)
                .HasDefaultValue(0)
                .HasPrecision(18,2)
                .IsRequired();

            builder.Property(g => g.LastSycendAt)
                .IsRequired();
        }
    }
}
