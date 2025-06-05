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
        CreateUser("admin", "admin@example.com", "Pham Van", "A", "1234567890", new List<string>{"123 Admin Street" }, "Male", 30, "123456", Guid.Parse("65863044-2837-441c-96c6-1425e83d60ea")),
        CreateUser("user", "user@example.com", "Nguyen Van", "B", "0987654321", new List<string>{"456 User Street" }, "Female", 25, "123456", Guid.Parse("d33545b7-0067-4a28-8670-872c7fccb8c4"))
    };

    private static User.API.Models.User CreateUser(string username, string email, string firstName, string lastName, string phone, List<string> address, string gender, int age, string password, Guid roleId)
    {
        var (hash, salt) = HashHelper.HashPassword(password);
        return new User.API.Models.User
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
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