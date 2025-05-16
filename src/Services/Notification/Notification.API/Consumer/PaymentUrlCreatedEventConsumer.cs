using BuildingBlocks.Messaging.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Notification.API.Consumer;

public class PaymentUrlCreatedEventConsumer : IConsumer<PaymentUrlCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public PaymentUrlCreatedEventConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<PaymentUrlCreatedEvent> context)
    {
        var evt = context.Message;

        var notification = new
        {
            Title = "Xin chúc mừng bạn",
            Message = $"Có đơn hàng mới {evt.OrderId}"
        };

        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
