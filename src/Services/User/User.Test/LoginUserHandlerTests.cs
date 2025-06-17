using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using User.API.User.Login;
using User.API.Models;
using User.API.Repository;

public class LoginUserHandlerTests
{
    [Fact]
    public async Task Handle_Success_ReturnsResult()
    {
        var mockRepo = new Mock<IUserRepository>();
        var mockConfig = new Mock<IConfiguration>();
        var password = "abc123";
        var (hash, salt) = User.API.Helpers.HashHelper.HashPassword(password);

        var user = new User.API.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };

        mockRepo.Setup(x => x.FindByEmailActiveAsync(user.Email)).ReturnsAsync(user);
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<User.API.Models.User>())).Returns(Task.CompletedTask);
        mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mockConfig.Setup(c => c["Jwt:Key"]).Returns("ThisIsASuperSuperSecretTestKey!1234");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("issuer");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("audience");

        var handler = new LoginUserHandler(mockRepo.Object, mockConfig.Object);

        // Act
        var result = await handler.Handle(new LoginUserCommand(user.Email, password), CancellationToken.None);

        // Assert
        Assert.NotNull(result.Token);
        Assert.IsType<User.API.Models.UserDto>(result.User);
        Assert.Equal(user.Email, ((User.API.Models.UserDto)result.User).Email);
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<User.API.Models.User>()), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var mockConfig = new Mock<IConfiguration>();
        var password = "abc123";
        var (hash, salt) = User.API.Helpers.HashHelper.HashPassword(password);

        var user = new User.API.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };

        mockRepo.Setup(x => x.FindByEmailActiveAsync(user.Email)).ReturnsAsync(user);

        var handler = new LoginUserHandler(mockRepo.Object, mockConfig.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginUserCommand(user.Email, "wrong_password"), CancellationToken.None));

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<User.API.Models.User>()), Times.Never);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var mockConfig = new Mock<IConfiguration>();

        mockRepo.Setup(x => x.FindByEmailActiveAsync(It.IsAny<string>())).ReturnsAsync((User.API.Models.User)null);

        var handler = new LoginUserHandler(mockRepo.Object, mockConfig.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginUserCommand("notfound@email.com", "any"), CancellationToken.None));

        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<User.API.Models.User>()), Times.Never);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}