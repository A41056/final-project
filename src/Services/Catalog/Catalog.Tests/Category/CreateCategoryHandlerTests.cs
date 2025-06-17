using Catalog.API.Categories.CreateCategory;
using Marten;
using Marten.Linq;
using Moq;
namespace Catalog.Test.Category;

public static class MartenQueryableAsyncExtensionsStub
{
    public static Task<List<T>> ToListAsync<T>(this IMartenQueryable<T> query, CancellationToken token = default)
    {
        return Task.FromResult(query.ToList());
    }
}

public class CreateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCategoriesAndReturnCreatedIds()
    {
        // Arrange
        var sessionMock = new Mock<IDocumentSession>();
        var handler = new CreateCategoryHandler(sessionMock.Object);

        var martQueryableMock = new Mock<IMartenQueryable<API.Models.Category>>();
        var emptyList = new List<API.Models.Category>();
        martQueryableMock.Setup(x => x.Provider).Returns(emptyList.AsQueryable().Provider);
        martQueryableMock.Setup(x => x.Expression).Returns(emptyList.AsQueryable().Expression);
        martQueryableMock.Setup(x => x.ElementType).Returns(emptyList.AsQueryable().ElementType);
        martQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => emptyList.AsQueryable().GetEnumerator());

        sessionMock.Setup(x => x.Query<API.Models.Category>()).Returns(martQueryableMock.Object);

        var storedCategories = new List<API.Models.Category>();
        sessionMock.Setup(x => x.Store(It.IsAny<API.Models.Category[]>()))
            .Callback<object[]>(cats =>
            {
                foreach (var cat in cats) storedCategories.Add((API.Models.Category)cat);
            });
        sessionMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateCategoryCommand("Cat1,Cat2", true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.CreatedIds.Count);
        Assert.Empty(result.Duplicates);
        sessionMock.Verify(x => x.Store(It.IsAny<API.Models.Category>()), Times.Exactly(2));
        sessionMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDuplicates_IfExists()
    {
        var sessionMock = new Mock<IDocumentSession>();
        var handler = new CreateCategoryHandler(sessionMock.Object);

        var data = new List<API.Models.Category> { new API.Models.Category { Name = "Cat1", Slug = "cat1" } };
        var martQueryableMock = new Mock<IMartenQueryable<API.Models.Category>>();
        martQueryableMock.Setup(x => x.Provider).Returns(data.AsQueryable().Provider);
        martQueryableMock.Setup(x => x.Expression).Returns(data.AsQueryable().Expression);
        martQueryableMock.Setup(x => x.ElementType).Returns(data.AsQueryable().ElementType);
        martQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => data.AsQueryable().GetEnumerator());

        sessionMock.Setup(x => x.Query<API.Models.Category>()).Returns(martQueryableMock.Object);

        var command = new CreateCategoryCommand("Cat1,Cat2", true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Single(result.CreatedIds);
        Assert.Contains("Cat1", result.Duplicates);
        sessionMock.Verify(x => x.Store(It.IsAny<API.Models.Category>()), Times.Once);
        sessionMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}