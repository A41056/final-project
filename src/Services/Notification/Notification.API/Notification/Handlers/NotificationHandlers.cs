using Notification.API.Data;
using static Notification.API.Notification.Queries.NotificationQueries;

namespace Notification.API.Notification.Handlers
{
    public class NotificationHandlers
    {
        public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, GetNotificationsResult>
        {
            private readonly INotificationRepository _repo;
            public GetNotificationsQueryHandler(INotificationRepository repo) => _repo = repo;

            public async Task<GetNotificationsResult> Handle(GetNotificationsQuery query, CancellationToken ct)
            {
                var notis = await _repo.GetNotifications(query.UserId, ct);
                return new GetNotificationsResult(notis);
            }
        }

        public class GetNotificationByIdQueryHandler : IQueryHandler<GetNotificationByIdQuery, Model.Notification?>
        {
            private readonly INotificationRepository _repo;
            public GetNotificationByIdQueryHandler(INotificationRepository repo) => _repo = repo;

            public Task<Model.Notification?> Handle(GetNotificationByIdQuery query, CancellationToken ct)
                => _repo.GetNotificationById(query.Id, ct);
        }

        public class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand, Model.Notification>
        {
            private readonly INotificationRepository _repo;
            public CreateNotificationCommandHandler(INotificationRepository repo) => _repo = repo;

            public Task<Model.Notification> Handle(CreateNotificationCommand command, CancellationToken ct)
            {
                var notification = new Model.Notification
                {
                    Id = Guid.NewGuid(),
                    Title = command.Title,
                    Message = command.Message,
                    UserId = command.UserId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                return _repo.AddNotification(notification, ct);
            }
        }

        public class MarkAllReadCommandHandler : ICommandHandler<MarkAllReadCommand, Unit>
        {
            private readonly INotificationRepository _repo;
            public MarkAllReadCommandHandler(INotificationRepository repo) => _repo = repo;

            public Task<Unit> Handle(MarkAllReadCommand command, CancellationToken ct)
                => _repo.MarkAllRead(command.UserId, ct);
        }

        public class MarkReadCommandHandler : ICommandHandler<MarkReadCommand, Unit>
        {
            private readonly INotificationRepository _repo;
            public MarkReadCommandHandler(INotificationRepository repo) => _repo = repo;

            public Task<Unit> Handle(MarkReadCommand command, CancellationToken ct)
                => _repo.MarkRead(command.Id, ct);
        }
    }
}
