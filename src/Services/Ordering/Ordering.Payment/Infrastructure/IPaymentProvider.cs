using Ordering.Payment.Infrastructure.Models;

namespace Ordering.Payment.Infrastructure
{
    public interface IPaymentProvider
    {
        Task<string> PaymentGenerateUrlAsync<TRequest>(TRequest model) where TRequest : IPaymentModel;
        Task<TransactionInfo> GetTransactionInfoAsync<TRequest>(TRequest model) where TRequest : ITransactionModel;
        Task<ConfirmResponse> ConfirmPaymentAsync<TRequest>(TRequest model) where TRequest : IConfirmModel;
    }
}
