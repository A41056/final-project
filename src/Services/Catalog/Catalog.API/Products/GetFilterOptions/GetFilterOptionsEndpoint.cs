namespace Catalog.API.Products.GetFilterOptions
{
    public class GetFilterOptionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/filter-options",
                async ([AsParameters] GetFilterOptionsQuery query, ISender sender) =>
                {
                    var result = await sender.Send(query);

                    return Results.Ok(result);
                })
                .WithName("GetFilterOptions")
                .Produces<FilterOptionsResult>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Get product filter options by category slug")
                .WithDescription("Returns available tags, price range and variant properties for a given category (by slug)");
        }
    }
}
