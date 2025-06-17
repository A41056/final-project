using FluentValidation;

namespace User.API.User.UpdateUser;

public record UpdateUserRequest(
    string? Username,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    List<string>? Address,
    string? Gender,
    int? Age
);
public record UpdateUserCommand(
    Guid Id,
    string? Username,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Phone,
    List<string>? Address,
    string? Gender,
    int? Age
) : IRequest<bool>;
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
internal class UpdateUserHandler(IDocumentSession session)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await session.LoadAsync<Models.User>(cmd.Id);
        if (user == null || !user.IsActive) throw new KeyNotFoundException("User not found");

        user.Username = cmd.Username ?? user.Username;
        user.Email = cmd.Email ?? user.Email;
        user.FirstName = cmd.FirstName ?? user.FirstName;
        user.LastName = cmd.LastName ?? user.LastName;
        user.Phone = cmd.Phone ?? user.Phone;
        user.Address = cmd.Address ?? user.Address;
        user.Gender = cmd.Gender ?? user.Gender;
        user.Age = cmd.Age ?? user.Age;
        user.ModifiedDate = DateTime.UtcNow;

        session.Store(user);
        await session.SaveChangesAsync(cancellationToken);

        return true;
    }
}
