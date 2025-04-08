using Payment.Enum;

namespace PaymentService.API.Payments.GeneratePaymentUrl;

public record GeneratePaymentUrlResponse(string PaymentUrl);
public record GeneratePaymentUrlCommand(string OrderCode, EOrderPaymentMethod PaymentMethod) : ICommand<GeneratePaymentUrlResult>;
public record GeneratePaymentUrlResult(string PaymentUrl);

internal class GeneratePaymentUrlHandler : ICommandHandler<GeneratePaymentUrlCommand, GeneratePaymentUrlResult>
{
    private readonly IPaymentService _paymentService;

    public GeneratePaymentUrlHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<GeneratePaymentUrlResult> Handle(GeneratePaymentUrlCommand command, CancellationToken cancellationToken)
    {
        var request = new PaymentGenerateUrlRequest
        {
            OrderCode = command.OrderCode,
            PaymentMethod = command.PaymentMethod,
        };

        var paymentUrl = await _paymentService.PaymentGenerateUrlAsync(request);
        return new GeneratePaymentUrlResult(paymentUrl);
    }
}