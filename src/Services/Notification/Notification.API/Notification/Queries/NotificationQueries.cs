namespace Notification.API.Notification.Queries
{
    public class NotificationQueries
    {
        public record GetNotificationsQuery(string UserId) : IQuery<GetNotificationsResult>;
        public record GetNotificationsResult(IReadOnlyList<Model.Notification> Notifications);

        public record GetNotificationByIdQuery(Guid Id) : IQuery<Model.Notification?>;

        public record CreateNotificationCommand(string Title, string Message, string? UserId) : ICommand<Model.Notification>;
        public record MarkAllReadCommand(string UserId) : ICommand;
        public record MarkReadCommand(Guid Id) : ICommand;
        public record DeleteNotificationCommand(Guid Id) : ICommand<bool>;
    }
}
