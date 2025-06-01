namespace Notification.API.Model
{
    public class Notification
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public string? UserId { get; set; }
    }
}
