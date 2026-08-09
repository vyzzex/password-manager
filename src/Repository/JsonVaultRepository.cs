using System.Text.Json;
using PasswordManager.Models;

namespace PasswordManager.Repository;

/// <summary>
/// Реализация хранилища на базе локального JSON-файла.
///
/// Стратегия безопасной записи (atomic write):
///   1. Сериализуем данные во временный файл (*.tmp) рядом с основным.
///   2. Заменяем основной файл временным через File.Move с флагом overwrite.
///   Это гарантирует, что при сбое питания/программы основной файл останется
///   в консистентном состоянии (либо старом, либо новом, но не «на полпути»).
/// </summary>
public sealed class JsonVaultRepository : IVaultRepository
{
    private readonly string _vaultPath;
    private readonly string _tempPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented    = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonVaultRepository(string vaultPath)
    {
        _vaultPath = vaultPath;
        _tempPath  = vaultPath + ".tmp";
    }

    /// <inheritdoc/>
    public bool VaultExists() => File.Exists(_vaultPath);

    /// <inheritdoc/>
    public VaultData? Load()
    {
        if (!File.Exists(_vaultPath))
            return null;

        try
        {
            var json = File.ReadAllText(_vaultPath);
            return JsonSerializer.Deserialize<VaultData>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException(
                $"Файл хранилища повреждён или имеет неверный формат: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public void Save(VaultData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);

        // Шаг 1: записываем во временный файл
        File.WriteAllText(_tempPath, json);

        // Шаг 2: атомарная замена основного файла
        File.Move(_tempPath, _vaultPath, overwrite: true);
    }
}
