using Ordering.Domain.Enums;

namespace Ordering.Application.Dtos;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    string OrderCode,
    string OrderName,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    EOrderStatus Status,
    List<OrderItemDto> OrderItems);
