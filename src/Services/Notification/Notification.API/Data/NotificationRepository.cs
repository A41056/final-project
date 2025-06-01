using Marten;

namespace Notification.API.Data
{
    public class NotificationRepository(IDocumentSession session) : INotificationRepository
    {
        public async Task<IReadOnlyList<Model.Notification>> GetNotifications(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await session.Query<Model.Notification>()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

            return notifications;
        }

        public Task<Model.Notification?> GetNotificationById(Guid id, CancellationToken cancellationToken = default)
            => session.LoadAsync<Model.Notification>(id, cancellationToken);

        public async Task<Model.Notification> AddNotification(Model.Notification notification, CancellationToken cancellationToken = default)
        {
            session.Store(notification);
            await session.SaveChangesAsync(cancellationToken);
            return notification;
        }

        public async Task<Unit> MarkAllRead(string userId, CancellationToken cancellationToken = default)
        {
            var unreadNotifications = await session.Query<Model.Notification>()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                session.Store(n);
            }

            await session.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        public async Task<Unit> MarkRead(Guid id, CancellationToken cancellationToken = default)
        {
            var notification = await session.LoadAsync<Model.Notification>(id, cancellationToken);
            if (notification is null) return Unit.Value;

            notification.IsRead = true;
            session.Store(notification);
            await session.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
