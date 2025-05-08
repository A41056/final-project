namespace Catalog.API.Categories.UpdateCategory;

using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public record UpdateCategoryRequest(
    Guid Id,
    string Name,
    string? Slug,
    Guid? ParentId,
    bool IsActive
);

public record UpdateCategoryResponse(bool IsSuccess);

public class UpdateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/categories/{id}",
            async (Guid id, UpdateCategoryRequest request, ISender sender) =>
            {
                if (id != request.Id)
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        detail: "ID in URL does not match ID in request body"
                    );
                }

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
            .WithDescription("Updates an existing category with the specified ID");
    }
}