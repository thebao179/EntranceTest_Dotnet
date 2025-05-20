using System.Security.Cryptography;
using System.Text;

namespace DotNetEntranceExam.Helpers
{
    public class HashHelper
    {
        public static string Sha1(string input)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha1.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }


    }
}
