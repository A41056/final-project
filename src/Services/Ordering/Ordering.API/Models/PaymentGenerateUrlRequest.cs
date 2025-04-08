using Payment.Enum;

namespace Payment.API.Models
{
    public class PaymentGenerateUrlRequest
    {
        public string OrderCode { get; set; }
        public EOrderPaymentMethod PaymentMethod { get; set; }
    }
}
