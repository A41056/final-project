namespace User.API.Repository;

public class UserRepository : IUserRepository
{
    private readonly IDocumentSession _session;
    public UserRepository(IDocumentSession session) => _session = session;

    public Task<Models.User?> FindByEmailActiveAsync(string email)
        => _session.Query<Models.User>().FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public Task<Models.User?> FindByEmailOrUsernameAsync(string email, string username)
        => _session.Query<Models.User>().FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

    public Task<Models.User?> FindByIdAsync(Guid id)
        => _session.LoadAsync<Models.User>(id);

    public async Task<List<Models.User>> GetActiveUsersAsync()
        => (await _session.Query<Models.User>().Where(u => u.IsActive).ToListAsync()).ToList();

    public Task InsertAsync(Models.User user)
    {
        _session.Store(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Models.User user)
    {
        _session.Store(user);
        return Task.CompletedTask;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _session.LoadAsync<Models.User>(id);
        if (user != null)
        {
            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;
            _session.Store(user);
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _session.SaveChangesAsync(cancellationToken);
}

