using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
               .HasConversion(id => id.Value, value => OrderItemId.Of(value))
               .ValueGeneratedNever();

        builder.Property(oi => oi.OrderId)
               .HasConversion(id => id.Value, value => OrderId.Of(value));

        builder.Property(oi => oi.ProductId)
               .HasConversion(id => id.Value, value => ProductId.Of(value));

        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.Price).HasPrecision(18, 2).IsRequired();

        // Map relationship with VariantProperties
        builder.HasMany(oi => oi.VariantProperties)
               .WithOne()
               .HasForeignKey(vp => vp.OrderItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}