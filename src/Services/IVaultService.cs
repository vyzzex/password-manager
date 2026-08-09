using PasswordManager.Models;

namespace PasswordManager.Services;

/// <summary>
/// Основной бизнес-сервис для работы с хранилищем паролей.
/// Объединяет криптографию, персистентность и доменную логику.
/// </summary>
public interface IVaultService
{
    /// <summary>Возвращает true, если файл хранилища уже создан.</summary>
    bool IsVaultCreated { get; }

    /// <summary>
    /// Создаёт новое хранилище с заданным мастер-паролем.
    /// Генерирует соль, дерайвирует ключ, шифрует canary-метку и сохраняет файл.
    /// </summary>
    void CreateVault(string masterPassword);

    /// <summary>
    /// Открывает существующее хранилище.
    /// Возвращает true, если мастер-пароль верен (canary совпала), иначе false.
    /// </summary>
    bool OpenVault(string masterPassword);

    /// <summary>Возвращает все записи (требует открытого хранилища).</summary>
    IReadOnlyList<PasswordEntry> GetAllEntries();

    /// <summary>Ищет записи по названию сервиса (регистронезависимо, частичное совпадение).</summary>
    IReadOnlyList<PasswordEntry> SearchByService(string query);

    /// <summary>Добавляет новую запись и сохраняет хранилище.</summary>
    void AddEntry(string service, string login, string password, string notes = "");

    /// <summary>Удаляет запись по Id и сохраняет хранилище.</summary>
    bool DeleteEntry(Guid id);

    /// <summary>Расшифровывает и возвращает логин записи.</summary>
    string DecryptLogin(PasswordEntry entry);

    /// <summary>Расшифровывает и возвращает пароль записи.</summary>
    string DecryptPassword(PasswordEntry entry);

    /// <summary>Расшифровывает и возвращает заметки записи.</summary>
    string DecryptNotes(PasswordEntry entry);
}
