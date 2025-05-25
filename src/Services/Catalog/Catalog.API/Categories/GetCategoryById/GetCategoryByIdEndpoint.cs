
namespace Catalog.API.Categorys.GetCategoryById;

//public record GetCategoryByIdRequest();
public record GetCategoryByIdResponse(Category Category);

public class GetCategoryByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetCategoryByIdQuery(id));

            var response = result.Adapt<GetCategoryByIdResponse>();

            return Results.Ok(response);
        })
        .WithName("GetCategoryById")
        .AllowAnonymous()
        .Produces<GetCategoryByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Category By Id")
        .WithDescription("Get Category By Id");
    }
}
