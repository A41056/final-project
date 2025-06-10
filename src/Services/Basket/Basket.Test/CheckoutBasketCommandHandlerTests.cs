using Basket.API.Basket.CheckoutBasket;
using Basket.API.Data;
using Basket.API.Dtos;
using Basket.API.Models;
using BuildingBlocks.Messaging.Events;
using MassTransit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Basket.Tests
{
    public class CheckoutBasketCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnCheckoutBasketResult_WhenBasketExists()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDto
            {
                UserId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<BasketItemDto>
                {
                    new BasketItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100 },
                    new BasketItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 200 }
                }
            };

            var command = new CheckoutBasketCommand(basketCheckoutDto);

            // Mock IBasketRepository trả về giỏ hàng hợp lệ
            var mockBasketRepository = new Mock<IBasketRepository>();
            var mockPublishEndpoint = new Mock<IPublishEndpoint>();

            var basket = new ShoppingCart(basketCheckoutDto.UserId)
            {
                Items = new List<ShoppingCartItem>
                {
                    new ShoppingCartItem { ProductId = basketCheckoutDto.Items[0].ProductId, Quantity = 2, Price = 100, ProductName = "Product 1", Color = "Red" },
                    new ShoppingCartItem { ProductId = basketCheckoutDto.Items[1].ProductId, Quantity = 1, Price = 200, ProductName = "Product 2", Color = "Blue" }
                }
            };

            mockBasketRepository.Setup(r => r.GetBasket(basketCheckoutDto.UserId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(basket);
            mockBasketRepository.Setup(r => r.DeleteBasket(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(true);

            mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            var handler = new CheckoutBasketCommandHandler(mockBasketRepository.Object, mockPublishEndpoint.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.PaymentUrl);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenBasketDoesNotExist()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDto
            {
                UserId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<BasketItemDto>
                {
                    new BasketItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 100 }
                }
            };

            var command = new CheckoutBasketCommand(basketCheckoutDto);

            // Mock IBasketRepository trả về null (không tìm thấy giỏ hàng)
            var mockBasketRepository = new Mock<IBasketRepository>();
            var mockPublishEndpoint = new Mock<IPublishEndpoint>();

            mockBasketRepository.Setup(r => r.GetBasket(basketCheckoutDto.UserId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync((ShoppingCart)null);

            var handler = new CheckoutBasketCommandHandler(mockBasketRepository.Object, mockPublishEndpoint.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.PaymentUrl);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenTimeoutOccursWhileWaitingForPaymentUrl()
        {
            // Arrange
            var basketCheckoutDto = new BasketCheckoutDto
            {
                UserId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<BasketItemDto>
        {
            new BasketItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 }
        }
            };

            var command = new CheckoutBasketCommand(basketCheckoutDto);

            // Mock IBasketRepository trả về giỏ hàng hợp lệ
            var mockBasketRepository = new Mock<IBasketRepository>();
            var mockPublishEndpoint = new Mock<IPublishEndpoint>();

            var basket = new ShoppingCart(basketCheckoutDto.UserId)
            {
                Items = new List<ShoppingCartItem>
        {
            new ShoppingCartItem { ProductId = basketCheckoutDto.Items[0].ProductId, Quantity = 1, Price = 100, ProductName = "Product 1", Color = "Red" }
        }
            };

            mockBasketRepository.Setup(r => r.GetBasket(basketCheckoutDto.UserId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(basket);
            mockBasketRepository.Setup(r => r.DeleteBasket(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(true);

            // Giả lập timeout khi chờ PaymentUrl
            var mockPaymentUrlTask = new TaskCompletionSource<string>(); // Giả lập TaskCompletionSource<string>
            mockPublishEndpoint.Setup(p => p.Publish(It.IsAny<BasketCheckoutEvent>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Giả lập tình huống timeout sau 3 giây
            var timeoutTask = Task.Delay(3000); // Thực hiện delay để mô phỏng timeout
            var completedTask = await Task.WhenAny(mockPaymentUrlTask.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                mockPaymentUrlTask.SetResult(null); // Khi timeout, set kết quả là null
            }

            var handler = new CheckoutBasketCommandHandler(mockBasketRepository.Object, mockPublishEndpoint.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.PaymentUrl); // Kiểm tra xem kết quả có đúng là null không
        }
    }
}
