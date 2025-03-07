using Marten.Schema;
using User.API.Helpers;

public class UserInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<User.API.Models.User>().AnyAsync())
            return;

        // Marten UPSERT will cater for existing records
        session.Store<User.API.Models.User>(GetPreconfiguredUsers());
        await session.SaveChangesAsync();
    }

    private static IEnumerable<User.API.Models.User> GetPreconfiguredUsers() => new List<User.API.Models.User>
    {
        CreateUser("admin", "admin@example.com", "1234567890", "123 Admin Street", "Male", 30, "123456", Guid.NewGuid()),
        CreateUser("user", "user@example.com", "0987654321", "456 User Street", "Female", 25, "123456", Guid.NewGuid())
    };

    private static User.API.Models.User CreateUser(string username, string email, string phone, string address, string gender, int age, string password, Guid roleId)
    {
        var (hash, salt) = HashHelper.HashPassword(password);
        return new User.API.Models.User
        {
            Username = username,
            Email = email,
            Phone = phone,
            Address = address,
            Gender = gender,
            Age = age,
            PasswordHash = hash,
            PasswordSalt = salt,
            RoleId = roleId
        };
    }
}