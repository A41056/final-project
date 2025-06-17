namespace User.API.User.DeactiveUser;

public class DeactiveUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{id:guid}",
            async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeactivateUserCommand(id));
                return result ? Results.Ok("User deactivated successfully") : Results.NotFound();
            })
            .WithName("DeactivateUser")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Deactivate user")
            .WithDescription("Deactivate (soft delete) user by id");
    }
}
