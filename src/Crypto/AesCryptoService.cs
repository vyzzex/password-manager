using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Crypto;

/// <summary>
/// Реализация криптографического сервиса на базе AES-256-CBC + PBKDF2.
///
/// Формат зашифрованных данных (Base64):
///   [ IV (16 байт) ][ Шифротекст (N байт) ]
///
/// Каждый вызов Encrypt генерирует новый случайный IV —
/// это гарантирует, что одинаковые открытые тексты дают разные шифротексты.
/// </summary>
public sealed class AesCryptoService : ICryptoService
{
    // PBKDF2: минимальное число итераций согласно рекомендациям OWASP 2023
    private const int Pbkdf2Iterations = 600_000;

    // Размер ключа AES-256 в байтах
    private const int KeySizeBytes = 32;

    // Размер IV для AES-CBC в байтах
    private const int IvSizeBytes = 16;

    /// <inheritdoc/>
    public byte[] GenerateSalt(int size = 32)
    {
        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    /// <inheritdoc/>
    public byte[] DeriveKey(string masterPassword, byte[] salt)
    {
        // PBKDF2 с SHA-512 — более стойкий против GPU-атак, чем SHA-1
        using var pbkdf2 = new Rfc2898DeriveBytes(
            masterPassword,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA512);

        return pbkdf2.GetBytes(KeySizeBytes);
    }

    /// <inheritdoc/>
    public string Encrypt(string plainText, byte[] key)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.KeySize  = 256;
        aes.Mode     = CipherMode.CBC;
        aes.Padding  = PaddingMode.PKCS7;
        aes.Key      = key;
        aes.GenerateIV(); // Новый IV для каждой операции — критически важно!

        using var encryptor = aes.CreateEncryptor();
        var plainBytes  = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Сохраняем IV вместе с шифротекстом: [IV][CipherText]
        var result = new byte[IvSizeBytes + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSizeBytes);
        Buffer.BlockCopy(cipherBytes, 0, result, IvSizeBytes, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc/>
    public string Decrypt(string cipherText, byte[] key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var allBytes = Convert.FromBase64String(cipherText);

        if (allBytes.Length < IvSizeBytes)
            throw new CryptographicException("Данные повреждены: слишком короткий шифротекст.");

        // Извлекаем IV и шифротекст
        var iv          = new byte[IvSizeBytes];
        var cipherBytes = new byte[allBytes.Length - IvSizeBytes];
        Buffer.BlockCopy(allBytes, 0, iv, 0, IvSizeBytes);
        Buffer.BlockCopy(allBytes, IvSizeBytes, cipherBytes, 0, cipherBytes.Length);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key     = key;
        aes.IV      = iv;

        using var decryptor  = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
