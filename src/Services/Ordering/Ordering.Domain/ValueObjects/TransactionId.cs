namespace Ordering.Domain.ValueObjects
{
    public record TransactionId
    {
        public Guid Value { get; }
        private TransactionId(Guid value) => Value = value;
        public static TransactionId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
                throw new DomainException("TransactionId cannot be empty.");
            return new TransactionId(value);
        }
    }
}
