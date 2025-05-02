using BuildingBlocks.Messaging.Events;
using MassTransit;
using System.Collections.Concurrent;

namespace Basket.API.EventHandler;

public class PaymentUrlCreatedEventConsumer : IConsumer<PaymentUrlCreatedEvent>
{
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _paymentUrlTcsMap = new();

    public static TaskCompletionSource<string> RegisterPaymentUrlTask(string userId)
    {
        var tcs = new TaskCompletionSource<string>();
        _paymentUrlTcsMap[userId] = tcs;
        return tcs;
    }

    public async Task Consume(ConsumeContext<PaymentUrlCreatedEvent> context)
    {
        var userId = context.Message.UserId;
        if (_paymentUrlTcsMap.TryGetValue(userId.ToString(), out var tcs))
        {
            tcs.TrySetResult(context.Message.PaymentUrl);
            _paymentUrlTcsMap.TryRemove(userId.ToString(), out _);
        }
        await Task.CompletedTask;
    }
}
