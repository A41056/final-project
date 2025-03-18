
namespace Catalog.API.Categorys.UpdateCategory;

public record UpdateCategoryRequest(Guid Id, string Name, List<string> Category, string Description, string ImageFile, decimal Price);
public record UpdateCategoryResponse(bool IsSuccess);

public class UpdateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories", 
            async (UpdateCategoryRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateCategoryCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateCategoryResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateCategory")
            .Produces<UpdateCategoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Category")
            .WithDescription("Update Category");
    }
}
