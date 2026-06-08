using System;
using System.Text;

namespace DBC.CareCommerce.Data.Security
{
    public sealed class DevelopmentTokenEncryptionService : ITokenEncryptionService
    {
        private const string Prefix = "DEVBASE64:";

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            var bytes = Encoding.UTF8.GetBytes(plainText);
            return Prefix + Convert.ToBase64String(bytes);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return encryptedText;
            }

            if (!encryptedText.StartsWith(Prefix, StringComparison.Ordinal))
            {
                // Backward compatibility for existing prototype rows that still contain raw tokens.
                return encryptedText;
            }

            var base64 = encryptedText.Substring(Prefix.Length);
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}