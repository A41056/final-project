using Moq;
using Review.API.Exceptions;
using Review.API.Repository;
using Review.API.Reviews.GetReviewById;
namespace Review.Test;

public class GetReviewByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_FoundReview_ReturnsReview()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new GetReviewByIdQueryHandler(mockRepo.Object);
        var reviewId = Guid.NewGuid();

        var review = new API.Models.Review
        {
            Id = reviewId,
            ProductId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            Rating = 5,
            Comment = "Great product!",
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        };

        mockRepo.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act
        var result = await handler.Handle(new GetReviewByIdQuery(reviewId), CancellationToken.None);

        // Assert
        Assert.Equal(reviewId, result.Review.Id);
        Assert.Equal("Test User", result.Review.UserName);
    }

    [Fact]
    public async Task Handle_ReviewNotFound_ThrowsException()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new GetReviewByIdQueryHandler(mockRepo.Object);
        var reviewId = Guid.NewGuid();

        mockRepo.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync((API.Models.Review)null);

        // Act & Assert
        await Assert.ThrowsAsync<ReviewNotFoundException>(() =>
            handler.Handle(new GetReviewByIdQuery(reviewId), CancellationToken.None));
    }
}