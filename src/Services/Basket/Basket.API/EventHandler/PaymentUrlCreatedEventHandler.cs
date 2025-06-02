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
        Console.WriteLine($"[Register] Registered TCS for user {userId}");
        return tcs;
    }

    public async Task Consume(ConsumeContext<PaymentUrlCreatedEvent> context)
    {
        var userId = context.Message.UserId;
        Console.WriteLine($"[Consume] PaymentUrlCreatedEvent received for user {userId}");
        if (_paymentUrlTcsMap.TryGetValue(userId.ToString(), out var tcs))
        {
            Console.WriteLine($"[Consume] Found TCS for user {userId}, setting result");
            tcs.TrySetResult(context.Message.PaymentUrl);
            _paymentUrlTcsMap.TryRemove(userId.ToString(), out _);
        }
        else
        {
            Console.WriteLine($"[Consume] No TCS found for user {userId}");
        }
        await Task.CompletedTask;
    }
}
