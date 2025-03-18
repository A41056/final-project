namespace Catalog.API.Categories.GetCategories;

public record GetCategoriesRequest(int? PageNumber = 1, int? PageSize = 10);
public record GetCategoriesResponse(IEnumerable<Category> Categories);

public class GetCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async ([AsParameters] GetCategoriesRequest request, ISender sender) =>
        {
            var query = request.Adapt<GetCategoriesQuery>();

            var result = await sender.Send(query);

            var response = result.Adapt<GetCategoriesResponse>();

            return Results.Ok(response);
        })
        .WithName("GetCategories")
        .RequireAuthorization()
        .Produces<GetCategoriesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Categories")
        .WithDescription("Get Categories");
    }
}
