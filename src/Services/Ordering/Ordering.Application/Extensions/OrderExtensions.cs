﻿namespace Ordering.Application.Extensions;

public static class OrderExtensions
{
    public static IEnumerable<OrderDto> ToOrderDtoList(this IEnumerable<Order> orders)
    {
        return orders.Select(order => new OrderDto(
            Id: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderCode: order.OrderCode,
            OrderName: order.OrderName.Value,
            ShippingAddress: new AddressDto(order.ShippingAddress.FirstName, order.ShippingAddress.LastName, order.ShippingAddress.EmailAddress!, order.ShippingAddress.AddressLine, order.ShippingAddress.Country, order.ShippingAddress.State, order.ShippingAddress.ZipCode),
            BillingAddress: new AddressDto(order.BillingAddress.FirstName, order.BillingAddress.LastName, order.BillingAddress.EmailAddress!, order.BillingAddress.AddressLine, order.BillingAddress.Country, order.BillingAddress.State, order.BillingAddress.ZipCode),
            Status: order.Status,
            OrderItems: order.OrderItems.Select(oi => new OrderItemDto(
                OrderId: oi.OrderId.Value,
                ProductId: oi.ProductId.Value,
                ProductName: oi.ProductName,
                Quantity: oi.Quantity,
                Price: oi.Price,
                VariantProperties: oi.VariantProperties.Select(vp => new VariantPropertyDto
                {
                    Type = vp.Type,
                    Value = vp.Value,
                    Image = vp.Image
                }).ToList()
            )).ToList()
        ));
    }

    public static OrderDto ToOrderDto(this Order order)
    {
        return DtoFromOrder(order);
    }

    private static OrderDto DtoFromOrder(Order order)
    {
        return new OrderDto(
            Id: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderCode: order.OrderCode,
            OrderName: order.OrderName.Value,
            ShippingAddress: new AddressDto(order.ShippingAddress.FirstName, order.ShippingAddress.LastName, order.ShippingAddress.EmailAddress!, order.ShippingAddress.AddressLine, order.ShippingAddress.Country, order.ShippingAddress.State, order.ShippingAddress.ZipCode),
            BillingAddress: new AddressDto(order.BillingAddress.FirstName, order.BillingAddress.LastName, order.BillingAddress.EmailAddress!, order.BillingAddress.AddressLine, order.BillingAddress.Country, order.BillingAddress.State, order.BillingAddress.ZipCode),
            Status: order.Status,
            OrderItems: order.OrderItems.Select(oi => new OrderItemDto(
                OrderId: oi.OrderId.Value,
                ProductId: oi.ProductId.Value,
                ProductName: oi.ProductName,
                Quantity: oi.Quantity,
                Price: oi.Price,
                VariantProperties: oi.VariantProperties.Select(vp => new VariantPropertyDto
                {
                    Type = vp.Type,
                    Value = vp.Value,
                    Image = vp.Image
                }).ToList()
            )).ToList()
        );
    }
}