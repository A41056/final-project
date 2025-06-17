using Microsoft.EntityFrameworkCore;
using Moq;
using Ordering.Application.Data;
using Ordering.Application.Dtos;
using Ordering.Application.Orders.Commands.CreateOrder;
using Ordering.Domain.Enums;
using Ordering.Domain.Models;
namespace Ordering.Test;

public class CreateOrderHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateOrderHandler(_dbContextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_AddOrder_And_SaveChanges()
    {
        // Arrange
        var orderDto = new OrderDto(
            Id: Guid.NewGuid(),
            CustomerId: Guid.NewGuid(),
            OrderCode: "ORD001",
            OrderName: "Test Order",
            ShippingAddress: new AddressDto("A", "B", "x@x.com", "123", "VN", "SG", "700000"),
            BillingAddress: new AddressDto("A", "B", "x@x.com", "123", "VN", "SG", "700000"),
            Status: EOrderStatus.Draft,
            OrderItems: new List<OrderItemDto>
            {
                new OrderItemDto(
                    OrderId: Guid.NewGuid(),
                    ProductId: Guid.NewGuid(),
                    ProductName: "Prod 1",
                    Quantity: 2,
                    Price: 10,
                    VariantProperties: new List<VariantPropertyDto>()
                )
            }
        );

        var dbSetMock = new Mock<DbSet<Order>>();
        _dbContextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

        var command = new CreateOrderCommand(orderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        dbSetMock.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.False(string.IsNullOrEmpty(result.OrderCode));
    }
}