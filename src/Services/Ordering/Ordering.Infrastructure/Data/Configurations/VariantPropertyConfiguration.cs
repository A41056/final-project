using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Data.Configurations
{
    public class VariantPropertyConfiguration : IEntityTypeConfiguration<VariantProperty>
    {
        public void Configure(EntityTypeBuilder<VariantProperty> builder)
        {
            builder.HasKey(vp => vp.Id);

            builder.Property(vp => vp.OrderItemId).HasConversion(
                orderItemId => orderItemId.Value,
                dbId => OrderItemId.Of(dbId));

            builder.Property(vp => vp.Type).HasMaxLength(50).IsRequired();
            builder.Property(vp => vp.Value).HasMaxLength(100).IsRequired();
            builder.Property(vp => vp.Image).HasMaxLength(200);
        }
    }
}
