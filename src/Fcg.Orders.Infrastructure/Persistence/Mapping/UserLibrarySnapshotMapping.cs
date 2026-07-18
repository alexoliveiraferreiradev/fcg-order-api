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
    public class UserLibrarySnapshotMapping : IEntityTypeConfiguration<UserLibrarySnapshot>
    {
        public void Configure(EntityTypeBuilder<UserLibrarySnapshot> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.GameId)
                .IsRequired();

            builder.Property(u => u.AcquiredAt)
                .IsRequired();

            builder.Property(u => u.LastSyncedAt)
                .IsRequired();
        }
    }
}
