using PasswordManager.Models;

namespace PasswordManager.Repository;

/// <summary>
/// Контракт для хранилища данных (персистентного слоя).
/// Позволяет заменить JSON-файл на любое другое хранилище без изменения бизнес-логики.
/// </summary>
public interface IVaultRepository
{
    /// <summary>Возвращает true, если файл хранилища уже существует.</summary>
    bool VaultExists();

    /// <summary>
    /// Загружает данные хранилища из файла.
    /// Возвращает null, если файл не найден или повреждён.
    /// </summary>
    VaultData? Load();

    /// <summary>
    /// Сохраняет данные хранилища в файл атомарно
    /// (через временный файл, чтобы исключить порчу данных при сбое).
    /// </summary>
    void Save(VaultData data);
}
