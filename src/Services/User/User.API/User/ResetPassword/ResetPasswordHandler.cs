using FluentValidation;
using User.API.Helpers;
using User.API.Models;
using User.API.Repository;
using User.API.Services;

namespace User.API.User.ResetPassword;
public record ResetPasswordRequest(string Email);
public record ResetPasswordCommand(string Email) : IRequest<bool>;
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
public class ResetPasswordHandler(
    IUserRepository repo,
    IEmailService emailService
) : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand cmd, CancellationToken cancellationToken)
    {
        var user = await repo.FindByEmailActiveAsync(cmd.Email);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        string newPassword = JwtHelper.GenerateRandomPassword(10);

        var (hash, salt) = HashHelper.HashPassword(newPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await repo.UpdateAsync(user);
        await repo.SaveChangesAsync(cancellationToken);

        var body = $"""
                        <p>Xin chào {user.FirstName + " " + user.LastName},</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu. Dưới đây là mật khẩu mới của bạn:</p>
                        <p><strong>{newPassword}</strong></p>
                        <p>Vui lòng đăng nhập và thay đổi mật khẩu ngay sau đó.</p>
                    """;

        await emailService.SendEmailAsync(new MailRequest
        {
            ToAddress = user.Email,
            Subject = "Mật khẩu mới của bạn",
            Body = body
        }, cancellationToken);

        return true;
    }
}
