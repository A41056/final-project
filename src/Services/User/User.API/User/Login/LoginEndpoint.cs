namespace User.API.User.Login;

public record LoginUserRequest(string Email, string Password);
public record LoginUserResponse(string Token, string RefreshToken, object User);

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/login",
            async (LoginUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<LoginUserCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<LoginUserResponse>();
                return Results.Ok(response);
            })
            .WithName("LoginUser")
            .Produces<LoginUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Login user")
            .WithDescription("User login endpoint");
    }
}
