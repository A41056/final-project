namespace Ordering.Payment.Infrastructure.Models
{
    public class ConfirmResponse
    {
        public string TransactionId { get; set; }
        public long Amount { get; set; }
        public string RspCode { get; set; }
        public string Message { get; set; }
        public string TransactionNo { get; set; }
        public string TransactionStatus { get; set; }
        public string BankCode { get; set; }
        public string BankTranNo { get; set; }
        public DateTime? PayDate { get; set; }
        public string PaymentContent { get; set; }
        public string CardType { get; set; }
    }
}
