using Payment.Infrastructure.Models;

namespace Payment.Models.VnPays
{
    public class VnpayTransactionModel : ITransactionModel
    {
        public DateTime PayDate { get; set; }
        public string OrderCode { get; set; }
        public string OrderInfo { get; set; }
    }
}
