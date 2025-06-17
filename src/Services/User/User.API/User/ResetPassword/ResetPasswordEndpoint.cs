namespace User.API.User.ResetPassword;

public class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/reset-password",
            async (ResetPasswordRequest request, ISender sender) =>
            {
                var command = request.Adapt<ResetPasswordCommand>();
                var result = await sender.Send(command);
                return result
                    ? Results.Ok("A new password has been sent to your email")
                    : Results.NotFound("User not found");
            })
            .WithName("ResetPassword")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Reset password")
            .WithDescription("Reset user password by email");
    }
}
