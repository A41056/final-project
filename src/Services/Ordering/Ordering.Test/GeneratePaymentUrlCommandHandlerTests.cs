using Moq;
using Ordering.Application.Dtos;
using Ordering.Application.Orders.Commands.GeneratePaymentUrl;
using Ordering.Payment.Common;
using Ordering.Payment.Infrastructure;
using Ordering.Payment.Models.VnPays;
using Ordering.Payment.Services;
namespace Ordering.Test;

public class GeneratePaymentUrlCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnPaymentUrl()
    {
        // Arrange
        var paymentProviderMock = new Mock<IPaymentProvider>();
        paymentProviderMock.Setup(x => x.PaymentGenerateUrlAsync(It.IsAny<VnpayPaymentModel>()))
            .ReturnsAsync("https://pay.url");

        var paymentFactoryMock = new Mock<IPaymentFactory>();
        paymentFactoryMock.Setup(x => x.CreatePaymentProvider(It.IsAny<EOrderPaymentMethod>()))
            .Returns(paymentProviderMock.Object);

        var handler = new GeneratePaymentUrlCommandHandler(paymentFactoryMock.Object);

        var items = new List<OrderItemDto>
        {
            new OrderItemDto(
                OrderId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                ProductName: "Product",
                Quantity: 1,
                Price: 20,
                VariantProperties: new List<VariantPropertyDto>()
            )
        };

        var command = new GeneratePaymentUrlCommand(
            OrderCode: "ORD001",
            PaymentMethod: EOrderPaymentMethod.VNPay,
            CustomerId: Guid.NewGuid(),
            UserName: "user01",
            EmailAddress: "abc@xyz.com",
            FirstName: "A",
            LastName: "B",
            AddressLine: "123 Main St",
            Country: "VN",
            State: "SG",
            ZipCode: "700000",
            Items: items
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://pay.url", result.PaymentUrl);
    }
}