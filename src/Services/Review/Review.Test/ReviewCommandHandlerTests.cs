using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using BuildingBlocks.Messaging.Events;
using Review.API.Exceptions;
using Review.API.Reviews.CreateReview;
using Review.API.Reviews.GetReviewByProductId;
using Review.API.Reviews.UpdateReview;
using BuildingBlocks.Pagination;
using MassTransit;
using Review.Test;

namespace Review.Tests
{
    public class ReviewCommandHandlerTests
    {
        [Fact]
        public async Task CreateReviewCommandHandler_ShouldReturnSuccess_WhenReviewIsCreated()
        {
            // Arrange
            var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), "testuser", 4, "Great product!");
            var mockSession = new Mock<IDocumentSession>();
            var mockPublishEndpoint = new Mock<IPublishEndpoint>();

            var handler = new CreateReviewCommandHandler(mockSession.Object, mockPublishEndpoint.Object);

            // Mock session behaviors
            mockSession.Setup(s => s.Store(It.IsAny<API.Models.Review>())).Verifiable();
            mockSession.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            mockSession.Setup(s => s.Query<API.Models.Review>())
                .Returns(new List<API.Models.Review>().AsQueryable().BuildMockQueryable());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeOfType<CreateReviewResult>();
            mockSession.Verify(s => s.Store(It.IsAny<API.Models.Review>()), Times.Once);
            mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<ProductRatingUpdatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReviewByProductQueryHandler_ShouldReturnReviews_WhenProductHasReviews()
        {
            // Arrange
            var query = new GetReviewByProductQuery(Guid.NewGuid());
            var mockSession = new Mock<IDocumentSession>();

            var reviews = new List<API.Models.Review>
            {
                new API.Models.Review { Id = Guid.NewGuid(), ProductId = query.ProductId, Rating = 4, Comment = "Great!" }
            };

            mockSession.Setup(s => s.Query<API.Models.Review>())
                .Returns(reviews.AsQueryable().BuildMockQueryable()); 
            mockSession.Setup(s => s.Query<API.Models.Review>().Count()).Returns(reviews.Count);

            var handler = new GetReviewByProductQueryHandler(mockSession.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Reviews.Should().NotBeEmpty();
            result.TotalItems.Should().Be(reviews.Count);
        }

        [Fact]
        public async Task GetReviewByProductQueryHandler_ShouldReturnEmpty_WhenProductHasNoReviews()
        {
            // Arrange
            var query = new GetReviewByProductQuery(Guid.NewGuid());
            var mockSession = new Mock<IDocumentSession>();

            mockSession.Setup(s => s.Query<API.Models.Review>())
                .Returns(Enumerable.Empty<API.Models.Review>().AsQueryable().BuildMockQueryable());
            mockSession.Setup(s => s.Query<API.Models.Review>().Count()).Returns(0);

            var handler = new GetReviewByProductQueryHandler(mockSession.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Reviews.Should().BeEmpty();
            result.TotalItems.Should().Be(0);
        }

        [Fact]
        public async Task UpdateReviewCommandHandler_ShouldReturnSuccess_WhenReviewIsUpdated()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var command = new UpdateReviewCommand(reviewId, Guid.NewGuid(), Guid.NewGuid(), 5, "Excellent product!");
            var mockSession = new Mock<IDocumentSession>();

            var review = new API.Models.Review
            {
                Id = reviewId,
                ProductId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Rating = 3,
                Comment = "Good product",
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            mockSession.Setup(s => s.LoadAsync<API.Models.Review>(reviewId, It.IsAny<CancellationToken>())).ReturnsAsync(review);
            mockSession.Setup(s => s.Update(It.IsAny<API.Models.Review>())).Verifiable();
            mockSession.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new UpdateReviewCommandHandler(mockSession.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            review.Rating.Should().Be(command.Rating);
            review.Comment.Should().Be(command.Comment);
            mockSession.Verify(s => s.Update(It.IsAny<API.Models.Review>()), Times.Once);
        }

        [Fact]
        public async Task UpdateReviewCommandHandler_ShouldThrowException_WhenReviewNotFound()
        {
            // Arrange
            var command = new UpdateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Excellent product!");
            var mockSession = new Mock<IDocumentSession>();

            mockSession.Setup(s => s.LoadAsync<API.Models.Review>(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((API.Models.Review)null);

            var handler = new UpdateReviewCommandHandler(mockSession.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ReviewNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}