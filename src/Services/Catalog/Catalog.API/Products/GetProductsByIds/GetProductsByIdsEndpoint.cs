namespace Catalog.API.Products.GetProductsByIds;

public record GetProductsByIdsRequest(List<Guid> ProductIds);

public record GetProductsByIdsResponse(List<Product> Products);

public class GetProductsByIdsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/bulk", async (GetProductsByIdsRequest request, ISender sender) =>
        {
            var query = new GetProductsByIdsQuery(request.ProductIds);
            var result = await sender.Send(query);

            var response = result.Adapt<GetProductsByIdsResponse>();

            return Results.Ok(response);
        })
        .WithName("GetProductsByIds")
        .Produces<GetProductsByIdsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Products By Ids")
        .WithDescription("Get multiple products by their IDs");
    }
}