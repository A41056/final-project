using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordering.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id)
                   .HasConversion(id => id.Value, value => TransactionId.Of(value))
                   .ValueGeneratedNever();
            builder.Property(t => t.OrderId)
                   .HasConversion(id => id.Value, value => OrderId.Of(value));
            builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(t => t.Status).HasMaxLength(50).IsRequired();
            builder.Property(t => t.TransactionDate).IsRequired();
            builder.HasOne<Order>()
                   .WithMany()
                   .HasForeignKey(t => t.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
