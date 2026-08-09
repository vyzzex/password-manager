namespace PasswordManager.Models;

/// <summary>
/// Одна запись в хранилище паролей.
/// Все чувствительные поля (логин, пароль, заметки) хранятся в зашифрованном виде.
/// </summary>
public sealed class PasswordEntry
{
    /// <summary>Уникальный идентификатор записи.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название сервиса/сайта (хранится в открытом виде для поиска).</summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>Зашифрованный логин (Base64).</summary>
    public string EncryptedLogin { get; set; } = string.Empty;

    /// <summary>Зашифрованный пароль (Base64).</summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>Зашифрованные заметки (Base64). Может быть пустым.</summary>
    public string EncryptedNotes { get; set; } = string.Empty;

    /// <summary>Дата создания записи.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего изменения.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
