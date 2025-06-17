using FluentValidation;
using User.API.Helpers;
using User.API.Models;
using User.API.Repository;

namespace User.API.User.Login;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;
public record LoginUserResult(string Token, string RefreshToken, object User);

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginUserHandler(IUserRepository repo, IConfiguration config)
    : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    public async Task<LoginUserResult> Handle(LoginUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await repo.FindByEmailActiveAsync(cmd.Email);
        if (user == null || !HashHelper.VerifyPassword(cmd.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid credentials");

        user.LoginFailedCount = 0;
        user.RefreshToken = Guid.NewGuid().ToString();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await repo.UpdateAsync(user);
        await repo.SaveChangesAsync(cancellationToken);

        var token = JwtHelper.GenerateJwtToken(user, config);
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Address = user.Address,
            Gender = user.Gender,
            Age = user.Age,
            RoleId = user.RoleId,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate
        };

        return new LoginUserResult(token, user.RefreshToken!, userDto);
    }
}
