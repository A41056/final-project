using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using User.API.User.ResetPassword;
using User.API.Models;
using User.API.Services;
using User.API.Repository;

public class ResetPasswordHandlerTests
{
    [Fact]
    public async Task Handle_Success_ResetsPassword_AndSendsMail()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var mockMail = new Mock<IEmailService>();
        var password = "old";
        var (hash, salt) = User.API.Helpers.HashHelper.HashPassword(password);

        var user = new User.API.Models.User
        {
            Id = Guid.NewGuid(),
            Email = "test@email.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        // Khi query thì trả về user
        mockRepo.Setup(x => x.FindByEmailActiveAsync(user.Email)).ReturnsAsync(user);
        mockRepo.Setup(x => x.UpdateAsync(It.IsAny<User.API.Models.User>())).Returns(Task.CompletedTask);
        mockRepo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mockMail.Setup(x => x.SendEmailAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var handler = new ResetPasswordHandler(mockRepo.Object, mockMail.Object);

        // Act
        var result = await handler.Handle(new ResetPasswordCommand(user.Email), CancellationToken.None);

        // Assert
        Assert.True(result);
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<User.API.Models.User>()), Times.Once);
        mockRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockMail.Verify(x => x.SendEmailAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_Throws()
    {
        var mockRepo = new Mock<IUserRepository>();
        var mockMail = new Mock<IEmailService>();

        // Chỉ cần mock repository method
        mockRepo.Setup(x => x.FindByEmailActiveAsync(It.IsAny<string>()))
            .ReturnsAsync((User.API.Models.User?)null);

        var handler = new ResetPasswordHandler(mockRepo.Object, mockMail.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new ResetPasswordCommand("notfound@email.com"), CancellationToken.None));
    }
}