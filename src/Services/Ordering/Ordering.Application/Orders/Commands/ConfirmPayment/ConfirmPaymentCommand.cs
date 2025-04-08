namespace Ordering.Application.Orders.Commands.ConfirmPayment
{
    public record ConfirmPaymentCommand(string QueryString) : IRequest<ConfirmPaymentResult>;

    public record ConfirmPaymentResult(
        string RspCode,
        string Message,
        string TransactionId,
        decimal Amount,
        string TransactionNo,
        string TransactionStatus,
        DateTime PayDate,
        string PaymentContent
    );
}
