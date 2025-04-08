using Ordering.Application.Orders.Commands.GeneratePaymentUrl;
using Ordering.Payment.Common;

namespace Ordering.API.Endpoints;

public class GeneratePaymentUrl : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment-generate-url", async (ISender sender, PaymentGenerateUrlRequest request) =>
        {
            var command = new GeneratePaymentUrlCommand(
                request.OrderCode,
                request.PaymentMethod,
                request.CustomerId,
                request.UserName,
                request.EmailAddress,
                request.FirstName,
                request.LastName,
                request.AddressLine,
                request.Country,
                request.State,
                request.ZipCode,
                request.Items
            );
            var result = await sender.Send(command);

            var response = new GeneratePaymentUrlResponse(result.PaymentUrl);
            return Results.Ok(response);
        })
        .WithName("GeneratePaymentUrl")
        .Produces<GeneratePaymentUrlResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Generate Payment URL")
        .WithDescription("Generates a payment URL for VNPay");
    }
}

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

public record GeneratePaymentUrlResponse(string PaymentUrl);
