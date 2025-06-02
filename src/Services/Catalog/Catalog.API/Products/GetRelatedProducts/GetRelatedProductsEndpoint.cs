namespace Catalog.API.Products.GetRelatedProducts
{
    public record GetRelatedProductsRequest(
    Guid ProductId,
    int? PageNumber = 1,
    int? PageSize = 10
);

    public record GetRelatedProductsResponse(
        IEnumerable<Product> Products,
        int TotalItems
    );

    public class GetRelatedProductsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/{productId:guid}/related", async (
                Guid productId,
                [AsParameters] GetRelatedProductsRequest request,
                ISender sender) =>
            {
                var query = new GetRelatedProductsQuery(
                    productId,
                    request.PageNumber,
                    request.PageSize
                );

                var result = await sender.Send(query);

                var response = result.Adapt<GetRelatedProductsResponse>();

                return Results.Ok(response);
            })
            .WithName("GetRelatedProducts")
            .AllowAnonymous()
            .Produces<GetRelatedProductsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get related products based on category and tags")
            .WithDescription("Returns products that share categories or tags with the specified product");
        }
    }

}
