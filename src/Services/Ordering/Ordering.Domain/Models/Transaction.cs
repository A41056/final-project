namespace Ordering.Domain.Models
{
    public class Transaction : Entity<TransactionId>
    {
        public OrderId OrderId { get; private set; }
        public decimal Amount { get; private set; }
        public string Status { get; private set; }
        public DateTime TransactionDate { get; private set; }

        private Transaction() { }

        public static Transaction Create(TransactionId id, OrderId orderId, decimal amount, string status, DateTime transactionDate)
        {
            return new Transaction
            {
                Id = id,
                OrderId = orderId,
                Amount = amount,
                Status = status,
                TransactionDate = transactionDate
            };
        }
    }
}
