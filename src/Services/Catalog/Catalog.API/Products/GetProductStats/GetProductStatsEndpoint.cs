namespace Catalog.API.Products.GetProductStats
{
    public record GetProductStatsRequest();

    public record ProductStatsDto(
        int Total,
        int Active,
        int Hot,
        int OutOfStock
    );

    public class GetProductStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/product-stats", async (ISender sender) =>
            {
                var result = await sender.Send(new GetProductStatsQuery());
                return Results.Ok(result);
            })
            .WithName("GetProductStats")
            .RequireAuthorization()
            .Produces<ProductStatsDto>(StatusCodes.Status200OK)
            .WithSummary("Get product statistics for dashboard")
            .WithDescription("Returns total, active, hot, and out-of-stock product counts");
        }
    }
}
