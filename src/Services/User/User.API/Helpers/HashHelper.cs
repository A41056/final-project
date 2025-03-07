using System.Security.Cryptography;

namespace User.API.Helpers;

public class HashHelper
{
    public static (byte[] PasswordHash, byte[] PasswordSalt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        return (hash, salt);
    }

    public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        return hash.SequenceEqual(storedHash);
    }
}
