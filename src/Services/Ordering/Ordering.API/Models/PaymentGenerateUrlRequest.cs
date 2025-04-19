using Ordering.Payment.Common;

namespace Ordering.API.Models
{
    public class PaymentGenerateUrlRequest
    {
        public string OrderCode { get; set; }
        public EOrderPaymentMethod PaymentMethod { get; set; }
    }
}
