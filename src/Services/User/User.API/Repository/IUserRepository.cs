namespace User.API.Repository;

public interface IUserRepository
{
    Task<Models.User?> FindByEmailActiveAsync(string email);
    Task<Models.User?> FindByEmailOrUsernameAsync(string email, string username);
    Task<Models.User?> FindByIdAsync(Guid id);
    Task<List<Models.User>> GetActiveUsersAsync();
    Task InsertAsync(Models.User user);
    Task UpdateAsync(Models.User user);
    Task DeactivateAsync(Guid id);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}