using Moq;
using Review.API.Repository;
using Review.API.Reviews.DeleteReview;
namespace Review.Test;

public class DeleteReviewCommandHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsTrue()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new DeleteReviewCommandHandler(mockRepo.Object);

        var reviewId = Guid.NewGuid();

        // Mock repository behavior for deleting the review
        mockRepo.Setup(x => x.DeleteAsync(reviewId)).Returns(Task.CompletedTask);
        mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(new DeleteReviewCommand(reviewId), CancellationToken.None);

        // Assert
        mockRepo.Verify(x => x.DeleteAsync(reviewId), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
    }
}