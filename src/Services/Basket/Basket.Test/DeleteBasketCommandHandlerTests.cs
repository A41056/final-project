using Basket.API.Basket.DeleteBasket;
using Basket.API.Data;
using Moq;

namespace Basket.Tests
{
    public class DeleteBasketCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenBasketIsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteBasketCommand(userId);

            // Mock IBasketRepository để trả về một kết quả xóa thành công
            var mockBasketRepository = new Mock<IBasketRepository>();
            mockBasketRepository.Setup(r => r.DeleteBasket(userId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(true);  // Giả lập xóa thành công

            var handler = new DeleteBasketCommandHandler(mockBasketRepository.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);  // Kiểm tra kết quả trả về là thành công
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenBasketDeleteFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteBasketCommand(userId);

            // Mock IBasketRepository để giả lập lỗi khi xóa giỏ hàng
            var mockBasketRepository = new Mock<IBasketRepository>();
            mockBasketRepository.Setup(r => r.DeleteBasket(userId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(false);  // Giả lập thất bại khi xóa giỏ hàng

            var handler = new DeleteBasketCommandHandler(mockBasketRepository.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);  // Kiểm tra kết quả trả về là thất bại
        }

        [Fact]
        public async Task Handle_ShouldCallDeleteBasketRepositoryMethod()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DeleteBasketCommand(userId);

            // Mock IBasketRepository để kiểm tra rằng phương thức DeleteBasket được gọi
            var mockBasketRepository = new Mock<IBasketRepository>();
            mockBasketRepository.Setup(r => r.DeleteBasket(userId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(true);

            var handler = new DeleteBasketCommandHandler(mockBasketRepository.Object);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockBasketRepository.Verify(r => r.DeleteBasket(userId, It.IsAny<CancellationToken>()), Times.Once);  // Kiểm tra xem phương thức DeleteBasket đã được gọi đúng 1 lần chưa
        }
    }
}
