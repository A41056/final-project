using Ordering.Payment.Common;

namespace Ordering.Application.Dtos
{
    public record PaymentGenerateUrlRequest(
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
);

    public record GeneratePaymentUrlResponse(
    string PaymentUrl
);
}
