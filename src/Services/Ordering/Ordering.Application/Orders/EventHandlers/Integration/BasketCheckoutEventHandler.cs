using BuildingBlocks.Messaging.Events;
using MassTransit;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Payment.Common;

namespace Ordering.Application.Orders.EventHandlers.Integration;

public class BasketCheckoutEventHandler : IConsumer<BasketCheckoutEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<BasketCheckoutEventHandler> _logger;

    public BasketCheckoutEventHandler(
        ISender sender,
        ILogger<BasketCheckoutEventHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        _logger.LogInformation("Integration Event handled: {IntegrationEvent}", context.Message.GetType().Name);

        var command = await MapToCreateOrderCommand(context.Message);
        var result = await _sender.Send(command);

        _logger.LogInformation("Draft order created: OrderId={OrderId}", result.Id);
    }

    private async Task<CreateOrderCommand> MapToCreateOrderCommand(BasketCheckoutEvent message)
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
            orderItems.Add(new OrderItemDto(
                OrderId: orderId,
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                Price: item.UnitPrice
            ));

            calculatedTotal += item.UnitPrice * item.Quantity;
        }

        if (calculatedTotal != message.TotalPrice)
        {
            _logger.LogWarning("TotalPrice mismatch: EventTotal={EventTotal}, CalculatedTotal={CalculatedTotal}", message.TotalPrice, calculatedTotal);
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
}

public interface IProductRepository
{
    Task<Product> GetByIdAsync(ProductId productId);
}