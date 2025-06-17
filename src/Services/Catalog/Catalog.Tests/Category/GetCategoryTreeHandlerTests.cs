using Moq;
using Catalog.API.Categories.GetCategoryTree;
using Catalog.API.Models;
using Marten;
using Marten.Linq;

public class GetCategoryTreeHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategoryTree()
    {
        // Arrange
        var sessionMock = new Mock<IDocumentSession>();

        var cat1 = new Category { Id = Guid.NewGuid(), Name = "Root", IsActive = true };
        var cat2 = new Category { Id = Guid.NewGuid(), Name = "Child", ParentId = cat1.Id, IsActive = true };
        var cat3 = new Category { Id = Guid.NewGuid(), Name = "Leaf", ParentId = cat2.Id, IsActive = true };

        var categories = new List<Category> { cat1, cat2, cat3 };

        // 1. Mock IMartenQueryable<Category>
        var martQueryableMock = new Mock<IMartenQueryable<Category>>();
        martQueryableMock.Setup(x => x.Provider).Returns(categories.AsQueryable().Provider);
        martQueryableMock.Setup(x => x.Expression).Returns(categories.AsQueryable().Expression);
        martQueryableMock.Setup(x => x.ElementType).Returns(categories.AsQueryable().ElementType);
        martQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => categories.AsQueryable().GetEnumerator());

        sessionMock.Setup(x => x.Query<Category>()).Returns(martQueryableMock.Object);

        // 2. Mock ToListAsync of Marten
        sessionMock.Setup(x => x.Query<Category>().ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var handler = new GetCategoryTreeHandler(sessionMock.Object);

        // Act
        var result = await handler.Handle(new GetCategoryTreeQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result); // Root node
        Assert.Single(result[0].Subcategories); // Child under root
        Assert.Single(result[0].Subcategories[0].Subcategories); // Leaf under child
    }
}