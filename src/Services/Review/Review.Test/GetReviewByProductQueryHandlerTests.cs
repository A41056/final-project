using Moq;
using Review.API.Repository;
using Review.API.Reviews.GetReviewByProductId;
namespace Review.Test;

public class GetReviewByProductQueryHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsPagedReviews()
    {
        // Arrange
        var mockRepo = new Mock<IReviewRepository>();
        var handler = new GetReviewByProductQueryHandler(mockRepo.Object);
        var productId = Guid.NewGuid();
        
        var reviews = new List<API.Models.Review>
        {
            new API.Models.Review { ProductId = productId, UserName = "User1", Rating = 5, Comment = "Great product!" },
            new API.Models.Review { ProductId = productId, UserName = "User2", Rating = 4, Comment = "Good product." }
        };

        mockRepo.Setup(x => x.GetByProductIdAsync(productId)).ReturnsAsync(reviews);

        // Act
        var result = await handler.Handle(new GetReviewByProductQuery(productId), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Reviews.Count());
        Assert.Equal(2, result.TotalItems);
    }
}