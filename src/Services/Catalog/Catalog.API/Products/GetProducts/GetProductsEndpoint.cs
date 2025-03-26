namespace Catalog.API.Products.GetProducts;

public record GetProductsRequest(
    int? PageNumber = 1,
    int? PageSize = 10,
    string? Search = null,
    string[]? CategoryIds = null,
    bool? IsHot = null,
    bool? IsActive = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null
);

public record GetProductsResponse(IEnumerable<Product> Products, int TotalItems);

public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([AsParameters] GetProductsRequest request, ISender sender) =>
        {
            var categoryIds = request.CategoryIds?.Select(Guid.Parse).ToArray();

            var query = new GetProductsQuery(
                request.PageNumber,
                request.PageSize,
                request.Search,
                categoryIds,
                request.IsHot,
                request.IsActive,
                request.CreatedFrom,
                request.CreatedTo
            );

            var result = await sender.Send(query);

            var response = result.Adapt<GetProductsResponse>();

            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .Produces<GetProductsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Products")
        .WithDescription("Get Products with filtering and pagination");
    }
}
