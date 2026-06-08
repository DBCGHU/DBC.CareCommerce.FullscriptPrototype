namespace DBC.CareCommerce.Data.Security
{
    public interface ITokenEncryptionService
    {
        string Encrypt(string plainText);

        string Decrypt(string encryptedText);
    }
}