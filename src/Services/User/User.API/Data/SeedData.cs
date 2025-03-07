using User.API.Helpers;

public static class SeedData
{
    public static void Initialize(IDocumentStore store)
    {
        using var session = store.LightweightSession();

        if (!session.Query<User.API.Models.User>().Any())
        {
            var (hash, salt) = HashHelper.HashPassword("Admin@123");
            var adminUser = new User.API.Models.User
            {
                Username = "admin",
                Email = "admin@example.com",
                Phone = "1234567890",
                Address = "123 Admin Street",
                Gender = "Male",
                Age = 30,
                PasswordHash = hash,
                PasswordSalt = salt,
                RoleId = Guid.NewGuid(), // Giả sử RoleId là một GUID mới
            };

            session.Store(adminUser);
            session.SaveChanges();
        }
    }
}