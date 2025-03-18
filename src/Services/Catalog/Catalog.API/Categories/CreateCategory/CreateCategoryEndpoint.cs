namespace Catalog.API.Categories.CreateCategory;

public class CreateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/categories", async (CreateCategoryCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .RequireAuthorization()
        .Produces<CreateCategoryResult>(StatusCodes.Status201Created)
        .WithSummary("Create a new category")
        .WithDescription("Create a new category");
    }
}
