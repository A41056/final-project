namespace Payment.API.GetTransactionInfo
{
    public class GetTransactionInfoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/transaction-info", async (ISender sender, [AsParameters] TransactionInfoRequest request) =>
            {
                var query = new GetTransactionInfoQuery(request.OrderCode);
                var result = await sender.Send(query);

                var response = new GetTransactionInfoResponse(result.Transactions);
                return Results.Ok(response);
            })
            .WithName("GetTransactionInfo")
            .Produces<GetTransactionInfoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Transaction Info")
            .WithDescription("Retrieves transaction information for a given order");
        }
    }
}
