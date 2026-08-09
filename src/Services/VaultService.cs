using System.Security.Cryptography;
using PasswordManager.Crypto;
using PasswordManager.Models;
using PasswordManager.Repository;

namespace PasswordManager.Services;

/// <summary>
/// Реализация бизнес-сервиса хранилища паролей.
///
/// Жизненный цикл ключа:
///   - Ключ (_derivedKey) живёт только в памяти, пока хранилище открыто.
///   - Он никогда не сериализуется и не попадает в файл.
///   - При завершении работы GC/финализатор очистит память (для продакшна
///     стоит использовать SecureString или MemoryProtection, но это выходит
///     за рамки данного учебного проекта).
/// </summary>
public sealed class VaultService : IVaultService
{
    // Контрольная метка — шифруем её при создании хранилища и проверяем при открытии
    private const string CanaryValue = "PASSWORD_MANAGER_CANARY_V1";

    private readonly ICryptoService    _crypto;
    private readonly IVaultRepository  _repository;

    private VaultData? _vaultData;
    private byte[]?    _derivedKey; // Ключ в памяти (никогда не записываем в файл!)

    public VaultService(ICryptoService crypto, IVaultRepository repository)
    {
        _crypto     = crypto;
        _repository = repository;
    }

    /// <inheritdoc/>
    public bool IsVaultCreated => _repository.VaultExists();

    /// <inheritdoc/>
    public void CreateVault(string masterPassword)
    {
        var salt = _crypto.GenerateSalt(32);
        var key  = _crypto.DeriveKey(masterPassword, salt);

        var vault = new VaultData
        {
            Salt            = Convert.ToBase64String(salt),
            EncryptedCanary = _crypto.Encrypt(CanaryValue, key),
            Entries         = []
        };

        _repository.Save(vault);

        // Сразу «открываем» хранилище после создания
        _vaultData   = vault;
        _derivedKey  = key;
    }

    /// <inheritdoc/>
    public bool OpenVault(string masterPassword)
    {
        var vault = _repository.Load()
            ?? throw new InvalidOperationException("Файл хранилища не найден.");

        var salt = Convert.FromBase64String(vault.Salt);
        var key  = _crypto.DeriveKey(masterPassword, salt);

        // Проверяем canary: если пароль неверный — расшифровка выбросит исключение
        // или вернёт мусор; сравниваем явно
        try
        {
            var decryptedCanary = _crypto.Decrypt(vault.EncryptedCanary, key);
            if (decryptedCanary != CanaryValue)
                return false;
        }
        catch (CryptographicException)
        {
            // Неверный пароль → AES выбросит CryptographicException (ошибка паддинга)
            return false;
        }

        _vaultData  = vault;
        _derivedKey = key;
        return true;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PasswordEntry> GetAllEntries()
    {
        EnsureVaultOpen();
        return _vaultData!.Entries.AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<PasswordEntry> SearchByService(string query)
    {
        EnsureVaultOpen();
        return _vaultData!.Entries
            .Where(e => e.Service.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    /// <inheritdoc/>
    public void AddEntry(string service, string login, string password, string notes = "")
    {
        EnsureVaultOpen();

        var entry = new PasswordEntry
        {
            Service           = service,
            EncryptedLogin    = _crypto.Encrypt(login, _derivedKey!),
            EncryptedPassword = _crypto.Encrypt(password, _derivedKey!),
            EncryptedNotes    = _crypto.Encrypt(notes, _derivedKey!)
        };

        _vaultData!.Entries.Add(entry);
        _repository.Save(_vaultData);
    }

    /// <inheritdoc/>
    public bool DeleteEntry(Guid id)
    {
        EnsureVaultOpen();

        var entry = _vaultData!.Entries.FirstOrDefault(e => e.Id == id);
        if (entry is null) return false;

        _vaultData.Entries.Remove(entry);
        _repository.Save(_vaultData);
        return true;
    }

    /// <inheritdoc/>
    public string DecryptLogin(PasswordEntry entry)
    {
        EnsureVaultOpen();
        return _crypto.Decrypt(entry.EncryptedLogin, _derivedKey!);
    }

    /// <inheritdoc/>
    public string DecryptPassword(PasswordEntry entry)
    {
        EnsureVaultOpen();
        return _crypto.Decrypt(entry.EncryptedPassword, _derivedKey!);
    }

    /// <inheritdoc/>
    public string DecryptNotes(PasswordEntry entry)
    {
        EnsureVaultOpen();
        return _crypto.Decrypt(entry.EncryptedNotes, _derivedKey!);
    }

    // Проверяет, что хранилище открыто перед операцией
    private void EnsureVaultOpen()
    {
        if (_vaultData is null || _derivedKey is null)
            throw new InvalidOperationException(
                "Хранилище не открыто. Сначала создайте или откройте его.");
    }
}
