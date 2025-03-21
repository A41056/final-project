
namespace Catalog.API.Categorys.DeleteCategory;

//public record DeleteCategoryRequest(Guid Id);
public record DeleteCategoryResponse(bool IsSuccess);

public class DeleteCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/categories/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteCategoryCommand(id));

            var response = result.Adapt<DeleteCategoryResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteCategory")
        .Produces<DeleteCategoryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Delete Category")
        .WithDescription("Delete Category");
    }
}
