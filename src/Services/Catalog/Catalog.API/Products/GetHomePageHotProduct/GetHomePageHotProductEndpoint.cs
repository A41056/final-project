namespace Catalog.API.Products.GetHomePageHotProduct;

public record GetTopHotProductsRequest(
    int? PageNumber = 1,
    int? PageSize = 10
);

public record GetTopHotProductsResponse(IEnumerable<Product> Products, int TotalItems);

public class GetTopHotProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/top-hot", async ([AsParameters] GetTopHotProductsRequest request, ISender sender) =>
        {
            var query = new GetTopHotProductsQuery(
                request.PageNumber,
                request.PageSize
            );

            var result = await sender.Send(query);

            var response = result.Adapt<GetTopHotProductsResponse>();

            return Results.Ok(response);
        })
        .WithName("GetTopHotProducts")
        .Produces<GetTopHotProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Top Hot Products")
        .WithDescription("Get a list of top hot products with pagination");
    }
}