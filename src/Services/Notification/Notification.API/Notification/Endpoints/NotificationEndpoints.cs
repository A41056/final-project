using Carter;
using static Notification.API.Notification.Queries.NotificationQueries;

namespace Notification.API.Notification.Endpoints
{
    public class NotificationEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/notifications/{userId}", async (string userId, ISender sender) =>
            {
                var result = await sender.Send(new GetNotificationsQuery(userId));
                return Results.Ok(result);
            })
            .WithName("GetNotifications")
            .Produces<GetNotificationsResult>(StatusCodes.Status200OK);

            app.MapGet("/notification/{id:guid}", async (Guid id, ISender sender) =>
            {
                var notification = await sender.Send(new GetNotificationByIdQuery(id));
                return notification is null ? Results.NotFound() : Results.Ok(notification);
            })
            .WithName("GetNotificationById")
            .Produces<Model.Notification>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            app.MapPost("/notifications", async (CreateNotificationCommand cmd, ISender sender) =>
            {
                var notification = await sender.Send(cmd);
                return Results.Created($"/notification/{notification.Id}", notification);
            })
            .WithName("CreateNotification")
            .Produces<Model.Notification>(StatusCodes.Status201Created);

            app.MapPost("/notifications/{userId}/mark-all-read", async (string userId, ISender sender) =>
            {
                await sender.Send(new MarkAllReadCommand(userId));
                return Results.NoContent();
            })
            .WithName("MarkAllRead")
            .Produces(StatusCodes.Status204NoContent);

            app.MapPost("/notification/{id:guid}/mark-read", async (Guid id, ISender sender) =>
            {
                await sender.Send(new MarkReadCommand(id));
                return Results.NoContent();
            })
            .WithName("MarkRead")
            .Produces(StatusCodes.Status204NoContent);

            //app.MapDelete("/notification/{id:guid}", async (Guid id, ISender sender) =>
            //{
            //    var deleted = await sender.Send(new DeleteNotificationCommand(id));
            //    return deleted ? Results.NoContent() : Results.NotFound();
            //})
            //.WithName("DeleteNotification")
            //.Produces(StatusCodes.Status204NoContent)
            //.Produces(StatusCodes.Status404NotFound);
        }
    }
}
