using Xunit;
using Moq;
using User.API.User.Register;
using User.API.Models;
using User.API.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsResult()
    {
        var mockRepo = new Mock<IUserRepository>();
        var cmd = new RegisterUserCommand(
            "testuser", "test@email.com", "Test", "User", "password123",
            "123456789", new System.Collections.Generic.List<string>(), "M", 22, Guid.NewGuid());

        // Không có user trùng
        mockRepo.Setup(x => x.FindByEmailOrUsernameAsync(cmd.Email, cmd.Username)).ReturnsAsync((User.API.Models.User?)null);
        mockRepo.Setup(x => x.InsertAsync(It.IsAny<User.API.Models.User>())).Returns(Task.CompletedTask);
        mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new RegisterUserHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(cmd.Username, result.Username);
        Assert.Equal(cmd.Email, result.Email);
        mockRepo.Verify(x => x.InsertAsync(It.IsAny<User.API.Models.User>()), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingUser_ThrowsInvalidOperation()
    {
        var mockRepo = new Mock<IUserRepository>();
        var cmd = new RegisterUserCommand(
            "testuser", "test@email.com", "Test", "User", "password123",
            "123456789", new System.Collections.Generic.List<string>(), "M", 22, Guid.NewGuid());

        var existingUser = new User.API.Models.User { Email = cmd.Email, Username = cmd.Username };
        mockRepo.Setup(x => x.FindByEmailOrUsernameAsync(cmd.Email, cmd.Username)).ReturnsAsync(existingUser);

        var handler = new RegisterUserHandler(mockRepo.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(cmd, CancellationToken.None));
    }
}