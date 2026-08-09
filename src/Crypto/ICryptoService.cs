namespace PasswordManager.Crypto;

/// <summary>
/// Контракт криптографического сервиса.
/// Изолирует логику шифрования от остальных слоёв приложения.
/// </summary>
public interface ICryptoService
{
    /// <summary>
    /// Генерирует криптографически стойкую случайную соль заданного размера.
    /// </summary>
    byte[] GenerateSalt(int size = 32);

    /// <summary>
    /// Дерайвирует ключ AES-256 из мастер-пароля и соли через PBKDF2.
    /// </summary>
    byte[] DeriveKey(string masterPassword, byte[] salt);

    /// <summary>
    /// Шифрует строку открытого текста, возвращает Base64-строку формата: [IV (16 байт)] + [шифротекст].
    /// </summary>
    string Encrypt(string plainText, byte[] key);

    /// <summary>
    /// Дешифрует Base64-строку, зашифрованную методом <see cref="Encrypt"/>.
    /// </summary>
    string Decrypt(string cipherText, byte[] key);
}
