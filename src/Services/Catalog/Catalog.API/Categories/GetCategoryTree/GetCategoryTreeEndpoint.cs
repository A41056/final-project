namespace Catalog.API.Categories.GetCategoryTree;

public class GetCategoryTreeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/tree", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCategoryTreeQuery());
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("GetCategoryTree")
        .Produces<List<CategoryDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Category Tree")
        .WithDescription("Returns a tree structure of all active categories");
    }
}
