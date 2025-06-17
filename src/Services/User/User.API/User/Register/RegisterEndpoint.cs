namespace User.API.User.Register;

public record RegisterUserRequest(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Phone,
    List<string> Address,
    string Gender,
    int Age,
    Guid RoleId
);
public record RegisterUserResponse(Guid Id, string Username, string Email);

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/register",
            async (RegisterUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<RegisterUserCommand>();
                var result = await sender.Send(command);
                var response = result.Adapt<RegisterUserResponse>();
                return Results.Created($"/users/{response.Id}", response);
            })
            .WithName("RegisterUser")
            .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register user")
            .WithDescription("Register a new user");
    }
}
