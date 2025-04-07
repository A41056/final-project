namespace Payment.API.ConfirmPayment
{
    public record ConfirmPaymentCommand(string DataQueryString) : ICommand<ConfirmPaymentResult>;
    public record ConfirmPaymentResult(string RspCode, string Message);

    internal class ConfirmPaymentHandler : ICommandHandler<ConfirmPaymentCommand, ConfirmPaymentResult>
    {
        private readonly IPaymentService _paymentService;

        public ConfirmPaymentHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<ConfirmPaymentResult> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
        {
            var result = await _paymentService.ConfirmPaymentAsync(command.DataQueryString);
            return new ConfirmPaymentResult(result.RspCode, result.Message);
        }
    }
}
