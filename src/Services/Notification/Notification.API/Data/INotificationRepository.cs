namespace Notification.API.Data
{
    public interface INotificationRepository
    {
        Task<IReadOnlyList<Model.Notification>> GetNotifications(string userId, CancellationToken cancellationToken = default);
        Task<Model.Notification?> GetNotificationById(Guid id, CancellationToken cancellationToken = default);
        Task<Model.Notification> AddNotification(Model.Notification notification, CancellationToken cancellationToken = default);
        Task<Unit> MarkAllRead(string userId, CancellationToken cancellationToken = default);
        Task<Unit> MarkRead(Guid id, CancellationToken cancellationToken = default);
    }
}
