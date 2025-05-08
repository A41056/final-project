namespace Catalog.API.Categories.GetCategoryPath;

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public class GetCategoryPathEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories/{id}/path", async (Guid id, ISender sender) =>
        {
            var query = new GetCategoryPathQuery(id);
            var result = await sender.Send(query);
            return Results.Ok(result.Path);
        })
        .WithName("GetCategoryPath")
        .RequireAuthorization()
        .Produces<List<Category>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Get category path")
        .WithDescription("Returns the full path of categories from root to the specified category");
    }
}