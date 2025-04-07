using Payment.Infrastructure.Models;

namespace Payment.Infrastructure
{
    public interface IPaymentProvider
    {
        Task<string> PaymentGenerateUrlAsync<TRequest>(TRequest model) where TRequest : IPaymentModel;
        Task<TransactionInfo> GetTransactionInfoAsync<TRequest>(TRequest model) where TRequest : ITransactionModel;
        Task<ConfirmResponse> ConfirmPaymentAsync<TRequest>(TRequest model) where TRequest : IConfirmModel;
    }
}
