using Ordering.Payment.Infrastructure.Models;

namespace Ordering.Payment.Infrastructure.Impls
{
    public abstract class PaymentProvider<TPay, TTran, TConfirm> : IPaymentProvider
        where TPay : IPaymentModel
        where TTran : ITransactionModel
        where TConfirm : IConfirmModel
    {
        public Task<string> PaymentGenerateUrlAsync<T>(T model) where T : IPaymentModel
        {
            return PaymentGenerateUrlAsync((TPay)(object)model);
        }

        public Task<TransactionInfo> GetTransactionInfoAsync<T>(T model) where T : ITransactionModel
        {
            return GetTransactionInfoAsync((TTran)(object)model);
        }

        public Task<ConfirmResponse> ConfirmPaymentAsync<T>(T model) where T : IConfirmModel
        {
            return ConfirmPaymentAsync((TConfirm)(object)model);
        }

        protected abstract Task<string> PaymentGenerateUrlAsync(TPay model);
        protected abstract Task<TransactionInfo> GetTransactionInfoAsync(TTran model);
        protected abstract Task<ConfirmResponse> ConfirmPaymentAsync(TConfirm model);
    }
}
