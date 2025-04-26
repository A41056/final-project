using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Enums;

namespace Ordering.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        // Map Value Object: OrderId
        builder.Property(o => o.Id)
               .HasConversion(id => id.Value, value => OrderId.Of(value))
               .ValueGeneratedNever();

        // Map Value Object: CustomerId
        builder.Property(o => o.CustomerId)
               .HasConversion(id => id.Value, value => CustomerId.Of(value));

        // Map Value Object: OrderName
        builder.Property(o => o.OrderName)
               .HasConversion(name => name.Value, value => OrderName.Of(value))
               .HasMaxLength(100);

        // Map Complex Type: Address (Owned Type)
        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(p => p.FirstName).HasColumnName("ShippingFirstName").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.LastName).HasColumnName("ShippingLastName").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.EmailAddress).HasColumnName("ShippingEmailAddress").HasMaxLength(100).IsRequired(false);
            a.Property(p => p.AddressLine).HasColumnName("ShippingAddressLine").HasMaxLength(200).IsRequired(false);
            a.Property(p => p.Country).HasColumnName("ShippingCountry").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.State).HasColumnName("ShippingState").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired(false);
        });

        builder.OwnsOne(o => o.BillingAddress, a =>
        {
            a.Property(p => p.FirstName).HasColumnName("BillingFirstName").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.LastName).HasColumnName("BillingLastName").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.EmailAddress).HasColumnName("BillingEmailAddress").HasMaxLength(100).IsRequired(false);
            a.Property(p => p.AddressLine).HasColumnName("BillingAddressLine").HasMaxLength(200).IsRequired(false);
            a.Property(p => p.Country).HasColumnName("BillingCountry").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.State).HasColumnName("BillingState").HasMaxLength(50).IsRequired(false);
            a.Property(p => p.ZipCode).HasColumnName("BillingZipCode").HasMaxLength(20).IsRequired(false);
        });

        // Map EOrderStatus (Enum)
        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasDefaultValue(EOrderStatus.Pending);

        // Map decimal TotalPrice
        builder.Property(o => o.TotalPrice)
               .HasPrecision(18, 2)
               .IsRequired();

        // Map OrderCode
        builder.Property(o => o.OrderCode)
               .HasMaxLength(50)
               .IsRequired();

        // Map PayDate and TransactionId
        builder.Property(o => o.PayDate).IsRequired();
        builder.Property(o => o.TransactionId).IsRequired();

        // Map relationship with OrderItems
        builder.HasMany(o => o.OrderItems)
               .WithOne()
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}