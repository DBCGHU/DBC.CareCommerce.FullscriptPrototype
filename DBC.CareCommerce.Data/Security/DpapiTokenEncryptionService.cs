using System;
using System.Security.Cryptography;
using System.Text;

namespace DBC.CareCommerce.Data.Security
{
    public sealed class DpapiTokenEncryptionService : ITokenEncryptionService
    {
        private const string Prefix = "DPAPI:";

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] protectedBytes = ProtectedData.Protect(
                plainBytes,
                null,
                DataProtectionScope.CurrentUser);

            return Prefix + Convert.ToBase64String(protectedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return encryptedText;
            }

            if (!encryptedText.StartsWith(Prefix, StringComparison.Ordinal))
            {
                // Backward compatibility for existing raw prototype token rows.
                return encryptedText;
            }

            string base64 = encryptedText.Substring(Prefix.Length);
            byte[] protectedBytes = Convert.FromBase64String(base64);

            byte[] plainBytes = ProtectedData.Unprotect(
                protectedBytes,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}