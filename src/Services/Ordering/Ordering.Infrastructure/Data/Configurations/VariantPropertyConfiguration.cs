using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.Data.Configurations;

public class VariantPropertyConfiguration : IEntityTypeConfiguration<VariantProperty>
{
    public void Configure(EntityTypeBuilder<VariantProperty> builder)
    {
        builder.ToTable("VariantProperties");
        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.OrderItemId)
               .HasConversion(id => id.Value, value => OrderItemId.Of(value));

        builder.Property(vp => vp.Type).HasMaxLength(50).IsRequired();
        builder.Property(vp => vp.Value).HasMaxLength(100).IsRequired();
        builder.Property(vp => vp.Image).HasMaxLength(200);
    }
}