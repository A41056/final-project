using Moq;
using Catalog.API.Categories.UpdateCategory;
using Catalog.API.Models;
using Marten;
using Marten.Linq;

public class UpdateCategoryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateCategory()
    {
        // Arrange
        var sessionMock = new Mock<IDocumentSession>();
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "OldName", Slug = "oldname", IsActive = true };

        sessionMock.Setup(x => x.LoadAsync<Category>(id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var emptyMartQueryable = new Mock<IMartenQueryable<Category>>();
        emptyMartQueryable.Setup(x => x.Provider).Returns(new List<Category>().AsQueryable().Provider);
        emptyMartQueryable.Setup(x => x.Expression).Returns(new List<Category>().AsQueryable().Expression);
        emptyMartQueryable.Setup(x => x.ElementType).Returns(new List<Category>().AsQueryable().ElementType);
        emptyMartQueryable.Setup(x => x.GetEnumerator()).Returns(() => new List<Category>().AsQueryable().GetEnumerator());
        sessionMock.Setup(x => x.Query<Category>()).Returns(emptyMartQueryable.Object);

        sessionMock.Setup(x => x.Update(It.IsAny<Category>()));
        sessionMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateCategoryCommandHandler(sessionMock.Object);

        var command = new UpdateCategoryCommand(
            id,
            "NewName",
            null,
            null,
            true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("NewName", category.Name);
        sessionMock.Verify(x => x.Update(category), Times.Once);
        sessionMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCategoryNotFound()
    {
        // Arrange
        var sessionMock = new Mock<IDocumentSession>();
        var handler = new UpdateCategoryCommandHandler(sessionMock.Object);

        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Name", null, null, true);

        sessionMock.Setup(x => x.LoadAsync<Category>(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null);

        var emptyMartQueryable = new Mock<IMartenQueryable<Category>>();
        emptyMartQueryable.Setup(x => x.Provider).Returns(new List<Category>().AsQueryable().Provider);
        emptyMartQueryable.Setup(x => x.Expression).Returns(new List<Category>().AsQueryable().Expression);
        emptyMartQueryable.Setup(x => x.ElementType).Returns(new List<Category>().AsQueryable().ElementType);
        emptyMartQueryable.Setup(x => x.GetEnumerator()).Returns(() => new List<Category>().AsQueryable().GetEnumerator());
        sessionMock.Setup(x => x.Query<Category>()).Returns(emptyMartQueryable.Object);

        // Act & Assert
        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}