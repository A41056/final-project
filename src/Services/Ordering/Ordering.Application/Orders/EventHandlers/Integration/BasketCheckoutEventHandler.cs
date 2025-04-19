using BuildingBlocks.Messaging.Events;
using MassTransit;
using NetTopologySuite.Index.HPRtree;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Payment.Common;
using System.Net.Http.Json;

namespace Ordering.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler : IConsumer<BasketCheckoutEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<BasketCheckoutEventHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory; // Để gọi Catalog Service
    private readonly IPublishEndpoint _publishEndpoint; // Để gửi PaymentUrlCreatedEvent

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

        // Bước 1: Xác thực sản phẩm với Catalog Service
        var products = await ValidateProducts(context.Message.Items);
        if (products == null)
        {
            _logger.LogError("Product validation failed for BasketCheckoutEvent");
            return; // Có thể gửi sự kiện lỗi nếu cần
        }

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

    private async Task<List<CatalogProductDto>?> ValidateProducts(List<BasketItem> items)
    {
        var client = _httpClientFactory.CreateClient("CatalogService");
        var productIds = items.Select(i => i.ProductId).ToList();
        var response = await client.PostAsJsonAsync("/api/products/bulk", productIds);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch products from Catalog Service");
            return null;
        }

        var products = await response.Content.ReadFromJsonAsync<List<CatalogProductDto>>();
        if (products == null || products.Count != items.Count)
        {
            _logger.LogError("Product count mismatch or null response from Catalog Service");
            return null;
        }

        // Kiểm tra giá và tồn kho
        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
            {
                _logger.LogError("Product not found: ProductId={ProductId}", item.ProductId);
                return null;
            }

            var variant = product.Variants.FirstOrDefault(v =>
                v.Properties.All(p => item.VariantProperties.Any(vp => vp.Type == p.Type && vp.Value == p.Value)));
            if (variant == null)
            {
                _logger.LogError("Variant not found for ProductId={ProductId}", item.ProductId);
                return null;
            }

            if (variant.Price != item.UnitPrice)
            {
                _logger.LogWarning("Price mismatch for ProductId={ProductId}. Expected={Expected}, Actual={Actual}",
                    item.ProductId, variant.Price, item.UnitPrice);
                return null;
            }

            if (variant.StockCount < item.Quantity)
            {
                _logger.LogError("Insufficient stock for ProductId={ProductId}. Requested={Requested}, Available={Available}",
                    item.ProductId, item.Quantity, variant.StockCount);
                return null;
            }
        }

        return products;
    }

    private async Task<CreateOrderCommand> MapToCreateOrderCommand(BasketCheckoutEvent message, List<CatalogProductDto> products)
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
        var paymentDto = new PaymentDto(
            message.CardName,
            message.CardNumber,
            message.Expiration,
            message.CVV,
            message.PaymentMethod
        );

        var orderItems = new List<OrderItemDto>();
        decimal calculatedTotal = 0;

        foreach (var item in message.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            var variant = product.Variants.First(v =>
                v.Properties.All(p => item.VariantProperties.Any(vp => vp.Type == p.Type && vp.Value == p.Value)));

            orderItems.Add(new OrderItemDto(
                OrderId: orderId,
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                Price: item.UnitPrice,
                VariantProperties: item.VariantProperties.Select(vp => new VariantPropertyDto
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
            Payment: paymentDto,
            Status: EOrderStatus.PauseForConfirmation,
            OrderItems: orderItems
        );

        return new CreateOrderCommand(orderDto);
    }

    private async Task<string> GeneratePaymentUrl(BasketCheckoutEvent message, Guid orderId)
    {
        var client = _httpClientFactory.CreateClient("OrderingService");
        var request = new PaymentGenerateUrlRequest(
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
                Price: i.UnitPrice,
                VariantProperties: i.VariantProperties.Select(vp => new VariantPropertyDto
                {
                    Type = vp.Type,
                    Value = vp.Value,
                    Image = vp.Image
                }).ToList()
            )).ToList()
        );

        var response = await client.PostAsJsonAsync("/payment-generate-url", request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to generate VNPay payment URL for OrderId: {OrderId}", orderId);
            return string.Empty;
        }

        var result = await response.Content.ReadFromJsonAsync<GeneratePaymentUrlResponse>();
        return result?.PaymentUrl ?? string.Empty;
    }
}