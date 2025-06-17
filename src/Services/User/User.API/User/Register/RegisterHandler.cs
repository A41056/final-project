using FluentValidation;
using MediatR;
using User.API.Helpers;
using User.API.Models;
using User.API.Repository;

namespace User.API.User.Register;

public record RegisterUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Phone,
    List<string> Address,
    string Gender,
    int Age,
    Guid RoleId
) : IRequest<RegisterUserResult>;

public record RegisterUserResult(Guid Id, string Username, string Email);

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(6);
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _repo;
    public RegisterUserHandler(IUserRepository repo) => _repo = repo;

    public async Task<RegisterUserResult> Handle(RegisterUserCommand cmd, CancellationToken cancellationToken)
    {
        var existingUser = await _repo.FindByEmailOrUsernameAsync(cmd.Email, cmd.Username);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists");

        var (hash, salt) = HashHelper.HashPassword(cmd.Password);
        var user = new Models.User
        {
            Username = cmd.Username,
            Email = cmd.Email,
            FirstName = cmd.FirstName,
            LastName = cmd.LastName,
            Phone = cmd.Phone,
            Address = cmd.Address,
            Gender = cmd.Gender,
            Age = cmd.Age,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = cmd.RoleId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _repo.InsertAsync(user);
        await _repo.SaveChangesAsync(cancellationToken);
        return new RegisterUserResult(user.Id, user.Username, user.Email);
    }
}
