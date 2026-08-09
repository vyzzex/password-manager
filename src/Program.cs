using PasswordManager.Crypto;
using PasswordManager.Repository;
using PasswordManager.Services;
using PasswordManager.UI;

/// <summary>
/// Точка входа в приложение.
/// Выполняет роль Composition Root — здесь собирается весь граф зависимостей
/// вручную (без DI-контейнера, т.к. для небольшого консольного приложения
/// это избыточно и только увеличивает сложность).
///
/// Принцип: зависимости передаются через конструкторы (Constructor Injection),
/// все компоненты зависят от абстракций (интерфейсов), а не конкретных реализаций.
/// </summary>

// Путь к файлу хранилища — рядом с исполняемым файлом
var vaultPath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "vault.json");

try
{
    // ─── Сборка зависимостей ──────────────────────────────────────────────────
    ICryptoService            cryptoService = new AesCryptoService();
    IVaultRepository          repository    = new JsonVaultRepository(vaultPath);
    IVaultService             vaultService  = new VaultService(cryptoService, repository);
    IPasswordGeneratorService generator     = new PasswordGeneratorService();

    var menu = new MenuHandler(vaultService, generator);

    // ─── Запуск ───────────────────────────────────────────────────────────────
    menu.Run();
}
catch (InvalidDataException ex)
{
    // Повреждённый JSON-файл
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[КРИТИЧЕСКАЯ ОШИБКА] Файл хранилища повреждён:");
    Console.WriteLine($"  {ex.Message}");
    Console.WriteLine($"\nПуть к файлу: {vaultPath}");
    Console.WriteLine("Создайте резервную копию и попробуйте восстановить файл вручную.");
    Console.ResetColor();
}
catch (IOException ex)
{
    // Проблемы с файловой системой
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[ОШИБКА ВВОДА/ВЫВОДА] Не удалось получить доступ к файлу:");
    Console.WriteLine($"  {ex.Message}");
    Console.ResetColor();
}
catch (Exception ex)
{
    // Непредвиденная ошибка
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n[НЕПРЕДВИДЕННАЯ ОШИБКА]");
    Console.WriteLine($"  {ex.GetType().Name}: {ex.Message}");
    Console.ResetColor();
}
finally
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\nНажмите любую клавишу для выхода...");
    Console.ResetColor();
    Console.ReadKey(intercept: true);
}
