using Moq;
using Review.API.Exceptions;
using Review.API.Repository;
using Review.API.Reviews.UpdateReview;
namespace Review.Test;

public class UpdateReviewCommandHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsTrue()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new UpdateReviewCommandHandler(mockRepo.Object);
        var reviewId = Guid.NewGuid();

        var review = new API.Models.Review
        {
            Id = reviewId,
            ProductId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            Rating = 4,
            Comment = "Great product!",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        };

        mockRepo.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(review);
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<API.Models.Review>())).Returns(Task.CompletedTask);
        mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(new UpdateReviewCommand(reviewId, review.ProductId, review.UserId, 5, "Updated review"), CancellationToken.None);

        // Assert
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<API.Models.Review>()), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsException()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new UpdateReviewCommandHandler(mockRepo.Object);
        var reviewId = Guid.NewGuid();

        mockRepo.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync((API.Models.Review)null);

        // Act & Assert
        await Assert.ThrowsAsync<ReviewNotFoundException>(() =>
            handler.Handle(new UpdateReviewCommand(reviewId, Guid.NewGuid(), Guid.NewGuid(), 5, "Updated review"), CancellationToken.None));
    }
}