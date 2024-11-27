using Notaion.Application.Interfaces.Services;
using System.Security.Cryptography;

namespace Notaion.Application.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService()
        {
            // Key và IV được mã hóa Base64
            string keyBase64 = "3Hg7vOPm82k/yJk3TZHDN6Nfp5YK9LtT2IvO3R4y6tE=";
            string ivBase64 = "abC6/DEeFGhijKLmNOPqrs==";

            _key = Convert.FromBase64String(keyBase64);
            _iv = Convert.FromBase64String(ivBase64);

            // Kiểm tra độ dài của Key và IV
            if (_key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes (256 bits).");
            if (_iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes (128 bits).");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plaintext cannot be null or empty.");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
            {
                writer.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty.");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedText));
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
