using Ordering.Payment.Models.VnPays;
using Ordering.Payment.Services;

namespace Ordering.Application.Orders.Commands.GeneratePaymentUrl
{
    public class GeneratePaymentUrlCommandHandler : IRequestHandler<GeneratePaymentUrlCommand, GeneratePaymentUrlResult>
    {
        private readonly IPaymentFactory _paymentFactory;

        public GeneratePaymentUrlCommandHandler(IPaymentFactory paymentFactory)
        {
            _paymentFactory = paymentFactory;
        }

        public async Task<GeneratePaymentUrlResult> Handle(GeneratePaymentUrlCommand request, CancellationToken cancellationToken)
        {
            var paymentProvider = _paymentFactory.CreatePaymentProvider(request.PaymentMethod);
            var vnpayModel = new VnpayPaymentModel
            {
                TxnRef = request.OrderCode,
                Amount = (long)request.Items.Sum(i => i.Price * i.Quantity),
                BillFullName = $"{request.FirstName} {request.LastName}".Trim(),
                BillEmail = request.EmailAddress,
                BillMobile = "N/A",
            };

            var paymentUrl = await paymentProvider.PaymentGenerateUrlAsync(vnpayModel);
            return new GeneratePaymentUrlResult(paymentUrl);
        }
    }
}
