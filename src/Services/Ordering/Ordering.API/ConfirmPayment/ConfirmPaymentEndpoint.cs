namespace Payment.API.ConfirmPayment
{
    public record ConfirmPaymentResponse(string RspCode, string Message);

    public class ConfirmPaymentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/confirm-payment", async (ISender sender, HttpContext context) =>
            {
                var queryString = context.Request.QueryString.Value;
                var command = new ConfirmPaymentCommand(queryString);
                var result = await sender.Send(command);

                var response = new ConfirmPaymentResponse(result.RspCode, result.Message);
                return Results.Ok(response);
            })
            .WithName("ConfirmPayment")
            .AllowAnonymous()
            .Produces<ConfirmPaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Confirm Payment")
            .WithDescription("Confirms a payment with VNPay and updates the order");
        }
    }
}
