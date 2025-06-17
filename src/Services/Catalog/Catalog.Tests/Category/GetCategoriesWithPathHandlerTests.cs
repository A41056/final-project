using Moq;
using Catalog.API.Categories.GetCategoriesPath;
using Catalog.API.Models;
using Marten;
using Marten.Linq; // Thêm using này

public class GetCategoriesWithPathHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategoriesWithPaths()
    {
        // Arrange
        var sessionMock = new Mock<IDocumentSession>();
        var handler = new GetCategoriesWithPathHandler(sessionMock.Object);

        var cat1 = new Category { Id = Guid.NewGuid(), Name = "Root", ParentId = null };
        var cat2 = new Category { Id = Guid.NewGuid(), Name = "Child", ParentId = cat1.Id };
        var cat3 = new Category { Id = Guid.NewGuid(), Name = "Leaf", ParentId = cat2.Id };

        var categories = new List<Category> { cat1, cat2, cat3 };

        // 1. Mock IMartenQueryable<Category>
        var martQueryableMock = new Mock<IMartenQueryable<Category>>();
        martQueryableMock.Setup(x => x.Provider).Returns(categories.AsQueryable().Provider);
        martQueryableMock.Setup(x => x.Expression).Returns(categories.AsQueryable().Expression);
        martQueryableMock.Setup(x => x.ElementType).Returns(categories.AsQueryable().ElementType);
        martQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => categories.AsQueryable().GetEnumerator());

        sessionMock.Setup(x => x.Query<Category>()).Returns(martQueryableMock.Object);

        // 2. Mock ToListAsync
        sessionMock.Setup(x => x.Query<Category>().ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await handler.Handle(new GetCategoriesWithPathQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Path.SequenceEqual(new[] { "Root" }));
        Assert.Contains(result, x => x.Path.SequenceEqual(new[] { "Root", "Child" }));
        Assert.Contains(result, x => x.Path.SequenceEqual(new[] { "Root", "Child", "Leaf" }));
    }
}