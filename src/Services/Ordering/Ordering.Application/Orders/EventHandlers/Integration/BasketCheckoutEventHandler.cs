using BuildingBlocks.Messaging.Events;
using MassTransit;
using NetTopologySuite.Index.HPRtree;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Application.Orders.Commands.GeneratePaymentUrl;
using Ordering.Domain.Enums;
using Ordering.Payment.Common;
using System.Net.Http.Json;

namespace Ordering.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler : IConsumer<BasketCheckoutEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<BasketCheckoutEventHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publishEndpoint;

    public BasketCheckoutEventHandler(
        ISender sender,
        ILogger<BasketCheckoutEventHandler> logger,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publishEndpoint)
    {
        _sender = sender;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        _logger.LogInformation("Processing BasketCheckoutEvent for UserId: {UserId}", context.Message.UserId);

        var products = context.Message.Items;

        // Bước 2: Tạo lệnh CreateOrderCommand
        var command = await MapToCreateOrderCommand(context.Message, products);
        var orderId = await _sender.Send(command);

        _logger.LogInformation("Draft order created: OrderId={OrderId}", orderId.Id);

        // Bước 3: Tạo URL thanh toán VNPay
        var paymentUrl = await GeneratePaymentUrl(context.Message, orderId.Id);
        if (string.IsNullOrEmpty(paymentUrl))
        {
            _logger.LogError("Failed to generate VNPay payment URL for OrderId: {OrderId}", orderId.Id);
            return; // Có thể gửi sự kiện lỗi nếu cần
        }

        // Bước 4: Gửi sự kiện PaymentUrlCreatedEvent để thông báo client
        var paymentUrlEvent = new PaymentUrlCreatedEvent
        {
            OrderId = orderId.Id,
            UserId = context.Message.UserId,
            PaymentUrl = paymentUrl
        };

        await _publishEndpoint.Publish(paymentUrlEvent, context.CancellationToken);
        _logger.LogInformation("Published PaymentUrlCreatedEvent for OrderId: {OrderId}", orderId.Id);
    }

    private async Task<CreateOrderCommand> MapToCreateOrderCommand(BasketCheckoutEvent message, List<BasketItem> products)
    {
        var orderId = Guid.NewGuid();
        var addressDto = new AddressDto(
            message.FirstName,
            message.LastName,
            message.EmailAddress,
            message.AddressLine,
            message.Country,
            message.State,
            message.ZipCode
        );
        
        var orderItems = new List<OrderItemDto>();
        decimal calculatedTotal = 0;

        foreach (var item in products)
        {
            orderItems.Add(new OrderItemDto(
                OrderId: orderId,
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                Price: item.UnitPrice,
                VariantProperties: item.VariantProperties.Select(vp => new Dtos.VariantPropertyDto
                {
                    Type = vp.Type,
                    Value = vp.Value,
                    Image = vp.Image
                }).ToList()
            ));

            calculatedTotal += item.UnitPrice * item.Quantity;
        }

        if (calculatedTotal != message.TotalPrice)
        {
            _logger.LogWarning("TotalPrice mismatch: EventTotal={EventTotal}, CalculatedTotal={CalculatedTotal}",
                message.TotalPrice, calculatedTotal);
        }

        var orderDto = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: message.UserName,
            ShippingAddress: addressDto,
            BillingAddress: addressDto,
            Status: EOrderStatus.Pending,
            OrderItems: orderItems
        );

        return new CreateOrderCommand(orderDto);
    }

    private async Task<string> GeneratePaymentUrl(BasketCheckoutEvent message, Guid orderId)
    {
        var command = new GeneratePaymentUrlCommand(
            OrderCode: orderId.ToString(),
            PaymentMethod: (EOrderPaymentMethod)message.PaymentMethod,
            CustomerId: message.CustomerId,
            UserName: message.UserName,
            EmailAddress: message.EmailAddress,
            FirstName: message.FirstName,
            LastName: message.LastName,
            AddressLine: message.AddressLine,
            Country: message.Country,
            State: message.State,
            ZipCode: message.ZipCode,
            Items: message.Items.Select(i => new OrderItemDto(
                OrderId: orderId,
                ProductId: i.ProductId,
                Quantity: i.Quantity,
                Price: i.UnitPrice * 1000,
                VariantProperties: i.VariantProperties.Select(vp => new Dtos.VariantPropertyDto
                {
                    Type = vp.Type,
                    Value = vp.Value,
                    Image = vp.Image
                }).ToList()
            )).ToList()
        );

        try
        {
            var result = await _sender.Send(command);
            return result.PaymentUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate VNPay payment URL for OrderId: {OrderId}", orderId);
            return string.Empty;
        }
    }
}