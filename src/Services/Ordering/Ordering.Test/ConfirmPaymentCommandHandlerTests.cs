using Microsoft.Extensions.Logging;
using Moq;
using Ordering.Application.Data;
using Ordering.Application.Orders.Commands.ConfirmPayment;
using Ordering.Payment.Common;
using Ordering.Payment.Infrastructure;
using Ordering.Payment.Infrastructure.Models;
using Ordering.Payment.Models.VnPays;
using Ordering.Payment.Services;
namespace Ordering.Test;

public class ConfirmPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentFactory> _paymentFactoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ILogger<ConfirmPaymentCommandHandler>> _loggerMock;
    private readonly ConfirmPaymentCommandHandler _handler;

    public ConfirmPaymentCommandHandlerTests()
    {
        _paymentFactoryMock = new Mock<IPaymentFactory>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<ConfirmPaymentCommandHandler>>();
        _handler = new ConfirmPaymentCommandHandler(
            _paymentFactoryMock.Object, 
            _dbContextMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnResult_WhenConfirmResponseIsNull()
    {
        // Arrange
        var paymentProviderMock = new Mock<IPaymentProvider>();
        paymentProviderMock.Setup(x => x.ConfirmPaymentAsync(It.IsAny<VnpayConfirmModel>()))
            .ReturnsAsync((ConfirmResponse?)null);

        _paymentFactoryMock.Setup(x => x.CreatePaymentProvider(It.IsAny<EOrderPaymentMethod>()))
            .Returns(paymentProviderMock.Object);
        
        var command = new ConfirmPaymentCommand("test");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Constants.VnPayResponseCode.OtherErrors, result.RspCode);
        Assert.Equal("Input data required", result.Message);
    }
}