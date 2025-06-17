namespace User.API.User.RefreshToken;
public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
public record RefreshTokenResult(string Token, string RefreshToken);
public class RefreshTokenEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/refresh-token",
            async (RefreshTokenRequest request, ISender sender) =>
            {
                var command = request.Adapt<RefreshTokenCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<RefreshTokenResponse>();
                return Results.Ok(response);
            })
            .WithName("RefreshToken")
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Refresh JWT token")
            .WithDescription("Generate a new JWT token from refresh token");
    }
}
