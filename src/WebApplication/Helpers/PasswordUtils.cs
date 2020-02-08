using System.Security.Cryptography;
using System.Text;

namespace WebApplication.Helpers
{
    public static class PasswordUtils
    {
        public static string Hash(string cleartext)
        {
            using (var algorithm = SHA256.Create())
            {
                var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(cleartext));

                return Encoding.UTF8.GetString(bytes);
            }
        }
        public static bool Compare(string hash, string cleartext)
        {
            return hash == Hash(cleartext);
        }
    }
}
