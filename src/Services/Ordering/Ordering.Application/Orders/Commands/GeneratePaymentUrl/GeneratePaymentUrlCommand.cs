using Ordering.Payment.Common;

namespace Ordering.Application.Orders.Commands.GeneratePaymentUrl
{
    public record GeneratePaymentUrlCommand(
    string OrderCode,
    EOrderPaymentMethod PaymentMethod,
    Guid CustomerId,
    string UserName,
    string EmailAddress,
    string FirstName,
    string LastName,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    List<OrderItemDto> Items
) : IRequest<GeneratePaymentUrlResult>;

    public record GeneratePaymentUrlResult(string PaymentUrl);
}
