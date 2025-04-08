using Ordering.API.ConfirmPayment;
using Payment.Infrastructure.Models;

namespace Ordering.API.Service
{
    public interface IPaymentService
    {
        Task<string> PaymentGenerateUrlAsync(PaymentGenerateUrlRequest request);
        Task<ConfirmPaymentResponse> ConfirmPaymentAsync(string vnpDataQueryString);
        Task<IEnumerable<TransactionInfo>> TransactionInfoAsync(TransactionInfoRequest request);
    }
}
