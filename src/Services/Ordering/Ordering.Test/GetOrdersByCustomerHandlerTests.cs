using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Ordering.Application.Data;
using Ordering.Application.Orders.Queries.GetOrdersByCustomer;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;
namespace Ordering.Test;

public class GetOrdersByCustomerHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOrdersOfCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var order1 = Order.Create(
            id: OrderId.Of(Guid.NewGuid()),
            customerId: CustomerId.Of(customerId),
            orderName: OrderName.Of("Order1"),
            shippingAddress: Address.Of("a", "b", "x@x.com", "123", "VN", "SG", "700000"),
            billingAddress: Address.Of("a", "b", "x@x.com", "123", "VN", "SG", "700000")
        );
        order1.Add(ProductId.Of(Guid.NewGuid()), "Prod 1", 1, 10m);

        var order2 = Order.Create(
            id: OrderId.Of(Guid.NewGuid()),
            customerId: CustomerId.Of(Guid.NewGuid()),
            orderName: OrderName.Of("Order2"),
            shippingAddress: Address.Of("a", "b", "x@x.com", "123", "VN", "SG", "700000"),
            billingAddress: Address.Of("a", "b", "x@x.com", "123", "VN", "SG", "700000")
        );

        var orders = new List<Order> { order1, order2 };

        var dbSetMock = orders.AsQueryable().BuildMockDbSet();

        var dbContextMock = new Mock<IApplicationDbContext>();
        dbContextMock.Setup(x => x.Orders).Returns(dbSetMock.Object);

        var handler = new GetOrdersByCustomerHandler(dbContextMock.Object);

        var query = new GetOrdersByCustomerQuery(customerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Orders, o => Assert.Equal(customerId, o.CustomerId));
    }
}