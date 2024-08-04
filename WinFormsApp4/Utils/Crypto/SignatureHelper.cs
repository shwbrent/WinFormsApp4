using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Utils.Crypto
{
    public static class SignatureHelper
    {
        private static readonly string key = "_Etai@Oven!Soft$2463#3531_"; // 請使用安全的密鑰

        public static string GenerateSignature(string content)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifySignature(string content, string signature)
        {
            var generatedSignature = GenerateSignature(content);
            return generatedSignature == signature;
        }
    }
}
