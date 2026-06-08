using System;

namespace DBC.CareCommerce.Data.Security
{
    public sealed class CompositeTokenEncryptionService : ITokenEncryptionService
    {
        private readonly ITokenEncryptionService _primaryEncryptionService;
        private readonly ITokenEncryptionService _developmentEncryptionService;

        public CompositeTokenEncryptionService(
            ITokenEncryptionService primaryEncryptionService,
            ITokenEncryptionService developmentEncryptionService)
        {
            if (primaryEncryptionService == null)
            {
                throw new ArgumentNullException("primaryEncryptionService");
            }

            if (developmentEncryptionService == null)
            {
                throw new ArgumentNullException("developmentEncryptionService");
            }

            _primaryEncryptionService = primaryEncryptionService;
            _developmentEncryptionService = developmentEncryptionService;
        }

        public string Encrypt(string plainText)
        {
            return _primaryEncryptionService.Encrypt(plainText);
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                return encryptedText;
            }

            if (encryptedText.StartsWith("DEVBASE64:", StringComparison.Ordinal))
            {
                return _developmentEncryptionService.Decrypt(encryptedText);
            }

            return _primaryEncryptionService.Decrypt(encryptedText);
        }
    }
}