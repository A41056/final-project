namespace User.API.User.UpdateUser;

public class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:guid}",
            async (Guid id, UpdateUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserCommand>() with { Id = id };
                var result = await sender.Send(command);
                return result ? Results.Ok("User updated successfully") : Results.NotFound();
            })
            .WithName("UpdateUser")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update user")
            .WithDescription("Update user by id");
    }
}
