namespace PaymentService.API.Payments.GeneratePaymentUrl;

public class GeneratePaymentUrlEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment-generate-url", async (ISender sender, PaymentGenerateUrlRequest request) =>
        {
            var command = new GeneratePaymentUrlCommand(request.OrderCode, request.PaymentMethod);
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