using Ordering.Application.Orders.Commands.ConfirmPayment;

namespace Ordering.API.Endpoints;

public record ConfirmPaymentResponse(
    string RspCode,
    string Message,
    string TransactionId,
    decimal Amount,
    string TransactionNo,
    string TransactionStatus,
    DateTime PayDate,
    string PaymentContent
);

public class ConfirmPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/confirm-payment", async (ISender sender, HttpContext context) =>
        {
            var queryString = context.Request.QueryString.Value;
            var command = new ConfirmPaymentCommand(queryString);
            var result = await sender.Send(command);

            var response = new ConfirmPaymentResponse(
                result.RspCode,
                result.Message,
                result.TransactionId,
                result.Amount,
                result.TransactionNo,
                result.TransactionStatus,
                result.PayDate,
                result.PaymentContent
            );
            return Results.Ok(response);
        })
        .WithName("ConfirmPayment")
        .RequireAuthorization()
        .Produces<ConfirmPaymentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Confirm Payment")
        .WithDescription("Confirms a payment with VNPay and updates the order");
    }
}
