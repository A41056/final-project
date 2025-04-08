using Ordering.Payment.Infrastructure.Models;

namespace Ordering.Payment.Models.VnPays
{
    public class VnpayTransactionModel : ITransactionModel
    {
        public DateTime PayDate { get; set; }
        public string OrderCode { get; set; }
        public string OrderInfo { get; set; }
    }
}
