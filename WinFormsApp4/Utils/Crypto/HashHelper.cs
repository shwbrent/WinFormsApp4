using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils.Crypto
{
    public static class HashHelper
    {
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 32;
        private static readonly int Iterations = 600_000;

        public static string HashPassword(string password)
        {
            byte[] saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes); // 使用 RandomNumberGenerator 生成隨機 salt

            var salt = Convert.ToBase64String(saltBytes);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            return $"{salt}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hashToVerify = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hash[i] != hashToVerify[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
