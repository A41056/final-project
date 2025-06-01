namespace Catalog.API.Categories.GetCategoriesPath
{
    public class GetCategoriesPathEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/categories/tree-with-path", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCategoriesWithPathQuery());
                return Results.Ok(result);
            })
            .WithName("GetCategoriesWithPath")
            .Produces<List<CategoryWithPath>>(StatusCodes.Status200OK)
            .RequireAuthorization();
        }
    }
}
