using Payment.Infrastructure.Models;

namespace Payment.API.GetTransactionInfo
{
    public record GetTransactionInfoQuery(string OrderCode) : IQuery<GetTransactionInfoResult>;
    public record GetTransactionInfoResult(IEnumerable<TransactionInfo> Transactions);
    public record GetTransactionInfoResponse(IEnumerable<TransactionInfo> Transactions);
    internal class GetTransactionInfoHandler : IQueryHandler<GetTransactionInfoQuery, GetTransactionInfoResult>
    {
        private readonly IPaymentService _paymentService;

        public GetTransactionInfoHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<GetTransactionInfoResult> Handle(GetTransactionInfoQuery query, CancellationToken cancellationToken)
        {
            var request = new TransactionInfoRequest { OrderCode = query.OrderCode };
            var transactions = await _paymentService.TransactionInfoAsync(request);
            return new GetTransactionInfoResult(transactions);
        }
    }
}
