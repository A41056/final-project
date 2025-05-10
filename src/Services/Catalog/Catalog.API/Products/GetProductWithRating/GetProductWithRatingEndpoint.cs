namespace Catalog.API.Products.GetProductWithRating;

public record GetProductsWithRatingRequest(List<Guid> ProductIds);

public record GetProductsWithRatingResponse(List<ProductWithRatingDto> Products);

public class GetProductsWithRatingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/bulk-with-rating", async (GetProductsWithRatingRequest request, ISender sender) =>
        {
            var query = new GetProductsWithRatingQuery(request.ProductIds);
            var result = await sender.Send(query);

            var response = result.Adapt<GetProductsWithRatingResponse>();

            return Results.Ok(response);
        })
        .WithName("GetProductsWithRating")
        .Produces<GetProductsWithRatingResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Products With Rating")
        .WithDescription("Get multiple products with their average ratings by their IDs");
    }
}
