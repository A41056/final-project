namespace User.API.User.DeactiveUser;

public record DeactivateUserCommand(Guid Id) : IRequest<bool>;
internal class DeactivateUserHandler(IDocumentSession session)
    : IRequestHandler<DeactivateUserCommand, bool>
{
    public async Task<bool> Handle(DeactivateUserCommand cmd, CancellationToken cancellationToken)
    {
        var user = await session.LoadAsync<Models.User>(cmd.Id);
        if (user == null) return false;

        user.IsActive = false;
        user.ModifiedDate = DateTime.UtcNow;
        session.Store(user);
        await session.SaveChangesAsync(cancellationToken);

        return true;
    }
}
