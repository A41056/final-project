namespace Ordering.Payment.Infrastructure.Models
{
    public class TransactionInfo
    {
        public string TxnRef { get; set; }
        public long Amount { get; set; }
        public string TransactionNo { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionStatus { get; set; }
        public string SecureHash { get; set; }
        public string BankCode { get; set; }
        public string OrderInfo { get; set; }
        public string PayDate { get; set; }
        public string Trace { get; set; }
        public string TransactionType { get; set; }
        public string Message { get; set; }
        public string CardType { get; set; }
    }
}
