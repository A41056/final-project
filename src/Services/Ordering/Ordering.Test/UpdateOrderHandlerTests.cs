using Microsoft.EntityFrameworkCore;
using Moq;
using Ordering.Application.Data;
using Ordering.Application.Dtos;
using Ordering.Application.Exceptions;
using Ordering.Application.Orders.Commands.UpdateOrder;
using Ordering.Domain.Enums;
using Ordering.Domain.Models;
namespace Ordering.Test;

public class UpdateOrderHandlerTests
{
    [Fact]
    public async Task Handle_Should_UpdateOrder_And_SaveChanges()
    {
        // Arrange
        var existingOrder = new Order();

        var dbSetMock = new Mock<DbSet<Order>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

        var handler = new UpdateOrderHandler(dbContextMock.Object);

        var shippingAddress = new AddressDto(
            FirstName: "John",
            LastName: "Doe",
            EmailAddress: "john@doe.com",
            AddressLine: "123 Main St",
            Country: "VN",
            State: "SG",
            ZipCode: "700000"
        );
        var billingAddress = new AddressDto(
            FirstName: "Jane",
            LastName: "Doe",
            EmailAddress: "jane@doe.com",
            AddressLine: "456 Another St",
            Country: "VN",
            State: "SG",
            ZipCode: "700000"
        );

        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(
                OrderId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ProductName: "Prod 1",
                Quantity: 2,
                Price: 10,
                VariantProperties: new List<VariantPropertyDto>()
            )
        };

        var orderDto = new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderCode: "ORDER123",
            OrderName: "Order Updated",
            ShippingAddress: shippingAddress,
            BillingAddress: billingAddress,
            Status: EOrderStatus.Completed,
            OrderItems: orderItems
        );

        var command = new UpdateOrderCommand(orderDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        dbSetMock.Verify(x => x.Update(existingOrder), Times.Once);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Should_ThrowException_IfOrderNotFound()
    {
        // Arrange
        var dbSetMock = new Mock<DbSet<Order>>();
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

        var handler = new UpdateOrderHandler(dbContextMock.Object);

        var shippingAddress = new AddressDto("A", "B", "x@x.com", "123", "VN", "SG", "700000");
        var billingAddress = new AddressDto("A", "B", "x@x.com", "123", "VN", "SG", "700000");
        var orderItems = new List<OrderItemDto>();

        var orderDto = new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderCode: "ORDER999",
            OrderName: "Order Not Exist",
            ShippingAddress: shippingAddress,
            BillingAddress: billingAddress,
            Status: EOrderStatus.Draft,
            OrderItems: orderItems
        );
        var command = new UpdateOrderCommand(orderDto);

        // Act & Assert
        await Assert.ThrowsAsync<OrderNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}