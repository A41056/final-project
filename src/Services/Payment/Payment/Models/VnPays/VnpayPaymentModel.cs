using Payment.Infrastructure.Models;

namespace Payment.Models.VnPays
{
    public class VnpayPaymentModel : IPaymentModel
    {
        public long Amount { get; set; }
        public string OrderInfo { get; set; }
        public string TxnRef { get; set; }
        public string BillMobile { get; set; }
        public string BillEmail { get; set; }
        public string BillFullName { get; set; }
    }
}
