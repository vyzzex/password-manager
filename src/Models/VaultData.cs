namespace PasswordManager.Models;

/// <summary>
/// Корневая структура JSON-файла хранилища.
/// Содержит соль, контрольный хэш мастер-пароля и список зашифрованных записей.
/// </summary>
public sealed class VaultData
{
    public int Version { get; set; } = 1;

    /// <summary>
    /// Соль (Base64), используемая при деривации ключа через PBKDF2.
    /// Генерируется один раз при создании хранилища.
    /// </summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>
    /// Зашифрованная «контрольная метка» (canary).
    /// При открытии хранилища расшифровываем её и сравниваем с константой —
    /// это позволяет мгновенно определить правильность мастер-пароля.
    /// </summary>
    public string EncryptedCanary { get; set; } = string.Empty;

    /// <summary>Список всех записей паролей.</summary>
    public List<PasswordEntry> Entries { get; set; } = [];
}
