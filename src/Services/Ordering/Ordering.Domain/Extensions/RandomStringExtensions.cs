namespace Ordering.Domain.Extensions
{
    using System;
    using System.Text;

    public static class RandomStringExtensions
    {
        private static readonly Random _random = new Random();

        public static string randomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }

            return sb.ToString();
        }
    }

}
