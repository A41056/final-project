namespace User.API.User.GetUserById;

public class GetUserByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetUserById")
            .Produces<GetUserByIdResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get user by id")
            .WithDescription("Get user detail by id");
    }
}
