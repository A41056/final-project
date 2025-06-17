using System.Security.Claims;
using User.API.Common;

namespace User.API.User.GetUsers;

public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users",
            async (HttpContext context, ISender sender) =>
            {
                var roleId = context.User.FindFirstValue("roleId");
                if (roleId != Constants.AdminRoleId)
                    return Results.Forbid();

                var result = await sender.Send(new GetUsersQuery(roleId!));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetUsers")
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Get all users (admin only)")
            .WithDescription("Admin get all user basic info");
    }
}
