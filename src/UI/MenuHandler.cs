using PasswordManager.Models;
using PasswordManager.Services;

namespace PasswordManager.UI;

/// <summary>
/// Обработчик интерактивного CLI-меню.
/// Полностью изолирует логику пользовательского интерфейса от бизнес-логики.
/// </summary>
public sealed class MenuHandler
{
    private readonly IVaultService            _vaultService;
    private readonly IPasswordGeneratorService _generator;

    public MenuHandler(IVaultService vaultService, IPasswordGeneratorService generator)
    {
        _vaultService = vaultService;
        _generator    = generator;
    }

    /// <summary>Запускает главный цикл приложения.</summary>
    public void Run()
    {
        Console.Clear();
        PrintBanner();

        // Инициализация: создание или открытие хранилища
        if (!InitializeVault())
            return;

        // Главный цикл меню
        while (true)
        {
            Console.Clear();
            PrintMainMenu();

            var choice = ConsoleHelper.ReadInt("Ваш выбор: ", 1, 6);

            switch (choice)
            {
                case 1: ShowAllEntries();      break;
                case 2: SearchEntries();       break;
                case 3: AddEntry();            break;
                case 4: DeleteEntry();         break;
                case 5: GeneratePassword();    break;
                case 6:
                    ConsoleHelper.WriteSuccess("До свидания! Хранилище закрыто.");
                    return;
            }
        }
    }

    // ─── Инициализация ────────────────────────────────────────────────────────

    private bool InitializeVault()
    {
        if (!_vaultService.IsVaultCreated)
        {
            ConsoleHelper.WriteWarning("Хранилище не найдено. Создаём новое...");
            return CreateNewVault();
        }
        else
        {
            return OpenExistingVault();
        }
    }

    private bool CreateNewVault()
    {
        ConsoleHelper.WriteHeader("Создание нового хранилища");
        ConsoleHelper.WriteLine("Придумайте надёжный мастер-пароль. Его нельзя восстановить!", ConsoleColor.Yellow);
        ConsoleHelper.WriteLine("Минимум 8 символов, используйте буквы, цифры и спецсимволы.", ConsoleColor.DarkGray);
        Console.WriteLine();

        string password, confirm;
        do
        {
            password = ConsoleHelper.ReadPassword("Новый мастер-пароль: ");
            if (password.Length < 8)
            {
                ConsoleHelper.WriteWarning("Пароль слишком короткий. Минимум 8 символов.");
                continue;
            }

            confirm = ConsoleHelper.ReadPassword("Подтвердите пароль:   ");
            if (password != confirm)
                ConsoleHelper.WriteWarning("Пароли не совпадают. Попробуйте снова.");

        } while (password.Length < 8 || password != confirm);

        try
        {
            ConsoleHelper.WriteLine("\nГенерируем ключ шифрования (это займёт несколько секунд)...", ConsoleColor.DarkGray);
            _vaultService.CreateVault(password);
            ConsoleHelper.WriteSuccess("Хранилище создано успешно!");
            ConsoleHelper.PressEnterToContinue();
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Не удалось создать хранилище: {ex.Message}");
            return false;
        }
    }

    private bool OpenExistingVault()
    {
        ConsoleHelper.WriteHeader("Открытие хранилища");

        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var password = ConsoleHelper.ReadPassword($"Мастер-пароль (попытка {attempt}/{maxAttempts}): ");

            try
            {
                ConsoleHelper.WriteLine("Проверяем пароль...", ConsoleColor.DarkGray);
                if (_vaultService.OpenVault(password))
                {
                    ConsoleHelper.WriteSuccess("Хранилище открыто!");
                    ConsoleHelper.PressEnterToContinue();
                    return true;
                }

                ConsoleHelper.WriteError("Неверный мастер-пароль.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Ошибка при открытии: {ex.Message}");
            }
        }

        ConsoleHelper.WriteError($"Превышено число попыток ({maxAttempts}). Приложение закрыто.");
        return false;
    }

    // ─── Меню ─────────────────────────────────────────────────────────────────

    private static void PrintBanner()
    {
        ConsoleHelper.WriteLine("""
        ╔════════════════════════════════════════╗
        ║        🔐  МЕНЕДЖЕР ПАРОЛЕЙ 🔐         ║
        ║       AES-256 · PBKDF2 · SHA-512       ║
        ╚════════════════════════════════════════╝
        """, ConsoleHelper.AccentColor);
    }

    private static void PrintMainMenu()
    {
        ConsoleHelper.WriteHeader("Главное меню");
        Console.WriteLine("  1. Просмотреть все записи");
        Console.WriteLine("  2. Поиск по сервису");
        Console.WriteLine("  3. Добавить запись");
        Console.WriteLine("  4. Удалить запись");
        Console.WriteLine("  5. Генератор паролей");
        Console.WriteLine("  6. Выход");
        ConsoleHelper.WriteSeparator();
    }

    // ─── Просмотр ─────────────────────────────────────────────────────────────

    private void ShowAllEntries()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Все записи");

        var entries = _vaultService.GetAllEntries();
        if (entries.Count == 0)
        {
            ConsoleHelper.WriteWarning("Хранилище пустое. Добавьте первую запись.");
        }
        else
        {
            PrintEntriesTable(entries);
            Console.WriteLine();
            ShowEntryDetail(entries);
        }

        ConsoleHelper.PressEnterToContinue();
    }

    private void SearchEntries()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Поиск по сервису");
        Console.Write("Введите название сервиса: ");
        var query = Console.ReadLine()?.Trim() ?? string.Empty;

        var results = _vaultService.SearchByService(query);

        if (results.Count == 0)
        {
            ConsoleHelper.WriteWarning($"По запросу «{query}» ничего не найдено.");
        }
        else
        {
            ConsoleHelper.WriteSuccess($"Найдено: {results.Count} запис(ей).");
            PrintEntriesTable(results);
            Console.WriteLine();
            ShowEntryDetail(results);
        }

        ConsoleHelper.PressEnterToContinue();
    }

    private void ShowEntryDetail(IReadOnlyList<PasswordEntry> entries)
    {
        if (!ConsoleHelper.ReadYesNo("Показать детали одной из записей?"))
            return;

        var num = ConsoleHelper.ReadInt($"Номер записи (1–{entries.Count}): ", 1, entries.Count);
        var entry = entries[num - 1];

        try
        {
            ConsoleHelper.WriteHeader($"Детали: {entry.Service}");
            Console.WriteLine($"  ID:       {entry.Id}");
            Console.WriteLine($"  Сервис:   {entry.Service}");
            Console.Write("  Логин:    ");
            ConsoleHelper.WriteLine(_vaultService.DecryptLogin(entry), ConsoleHelper.AccentColor);
            Console.Write("  Пароль:   ");
            ConsoleHelper.WriteLine(_vaultService.DecryptPassword(entry), ConsoleHelper.SuccessColor);

            var notes = _vaultService.DecryptNotes(entry);
            if (!string.IsNullOrWhiteSpace(notes))
            {
                Console.Write("  Заметки:  ");
                ConsoleHelper.WriteLine(notes, ConsoleColor.White);
            }

            Console.WriteLine($"  Создано:  {entry.CreatedAt:dd.MM.yyyy HH:mm} UTC");

            ConsoleHelper.WriteWarning("\n⚠️  Закройте терминал после просмотра конфиденциальных данных!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Не удалось расшифровать запись: {ex.Message}");
        }
    }

    // ─── Добавление ───────────────────────────────────────────────────────────

    private void AddEntry()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Добавить запись");

        var service = ConsoleHelper.ReadRequiredLine("Сервис/сайт:  ");
        var login   = ConsoleHelper.ReadRequiredLine("Логин/email:  ");

        string password;
        if (ConsoleHelper.ReadYesNo("Сгенерировать пароль автоматически?"))
        {
            var opts = AskGeneratorOptions();
            password = _generator.Generate(opts);
            Console.Write("Сгенерированный пароль: ");
            ConsoleHelper.WriteLine(password, ConsoleHelper.SuccessColor);
        }
        else
        {
            password = ConsoleHelper.ReadPassword("Пароль:       ");
            if (string.IsNullOrEmpty(password))
            {
                ConsoleHelper.WriteWarning("Пароль не может быть пустым. Операция отменена.");
                ConsoleHelper.PressEnterToContinue();
                return;
            }
        }

        Console.Write("Заметки (необязательно): ");
        var notes = Console.ReadLine()?.Trim() ?? string.Empty;

        try
        {
            _vaultService.AddEntry(service, login, password, notes);
            ConsoleHelper.WriteSuccess($"Запись «{service}» успешно добавлена!");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Не удалось сохранить запись: {ex.Message}");
        }

        ConsoleHelper.PressEnterToContinue();
    }

    // ─── Удаление ─────────────────────────────────────────────────────────────

    private void DeleteEntry()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Удалить запись");

        var entries = _vaultService.GetAllEntries();
        if (entries.Count == 0)
        {
            ConsoleHelper.WriteWarning("Нет записей для удаления.");
            ConsoleHelper.PressEnterToContinue();
            return;
        }

        PrintEntriesTable(entries);
        Console.WriteLine();

        var num   = ConsoleHelper.ReadInt($"Номер для удаления (1–{entries.Count}): ", 1, entries.Count);
        var entry = entries[num - 1];

        ConsoleHelper.WriteWarning($"Вы собираетесь удалить: «{entry.Service}»");
        if (!ConsoleHelper.ReadYesNo("Подтвердите удаление"))
        {
            ConsoleHelper.WriteLine("Отменено.", ConsoleColor.DarkGray);
            ConsoleHelper.PressEnterToContinue();
            return;
        }

        try
        {
            _vaultService.DeleteEntry(entry.Id);
            ConsoleHelper.WriteSuccess($"Запись «{entry.Service}» удалена.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Не удалось удалить запись: {ex.Message}");
        }

        ConsoleHelper.PressEnterToContinue();
    }

    // ─── Генератор ────────────────────────────────────────────────────────────

    private void GeneratePassword()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Генератор надёжных паролей");

        var opts = AskGeneratorOptions();

        try
        {
            var generated = _generator.Generate(opts);
            Console.WriteLine();
            ConsoleHelper.WriteLine("Сгенерированный пароль:", ConsoleColor.White);
            ConsoleHelper.WriteLine($"  {generated}", ConsoleHelper.SuccessColor);

            // Простая оценка надёжности
            Console.WriteLine();
            PrintPasswordStrength(generated);
        }
        catch (ArgumentException ex)
        {
            ConsoleHelper.WriteError(ex.Message);
        }

        ConsoleHelper.PressEnterToContinue();
    }

    private static PasswordGeneratorOptions AskGeneratorOptions()
    {
        Console.WriteLine();
        var length = ConsoleHelper.ReadInt("Длина пароля (8–128): ", 8, 128);

        var opts = new PasswordGeneratorOptions
        {
            Length       = length,
            UseUppercase = ConsoleHelper.ReadYesNo("Заглавные буквы (A–Z)?"),
            UseLowercase = ConsoleHelper.ReadYesNo("Строчные буквы (a–z)?"),
            UseDigits    = ConsoleHelper.ReadYesNo("Цифры (0–9)?"),
            UseSpecial   = ConsoleHelper.ReadYesNo("Спецсимволы (!@#...)?")
        };

        // Если ничего не выбрано — включаем всё
        if (!opts.UseUppercase && !opts.UseLowercase && !opts.UseDigits && !opts.UseSpecial)
        {
            ConsoleHelper.WriteWarning("Ничего не выбрано — включены все символы.");
            opts.UseUppercase = opts.UseLowercase = opts.UseDigits = opts.UseSpecial = true;
        }

        return opts;
    }

    // ─── Вспомогательные методы UI ────────────────────────────────────────────

    private static void PrintEntriesTable(IReadOnlyList<PasswordEntry> entries)
    {
        ConsoleHelper.WriteSeparator();
        ConsoleHelper.WriteLine(
            $"  {"№",-4} {"Сервис",-25} {"Создано",-20}",
            ConsoleHelper.AccentColor);
        ConsoleHelper.WriteSeparator();

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            Console.WriteLine($"  {i + 1,-4} {e.Service,-25} {e.CreatedAt:dd.MM.yyyy HH:mm}");
        }

        ConsoleHelper.WriteSeparator();
    }

    private static void PrintPasswordStrength(string password)
    {
        // Упрощённая эвристическая оценка
        int score = 0;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(c => !char.IsLetterOrDigit(c))) score++;

        var (label, color) = score switch
        {
            <= 2 => ("Слабый",   ConsoleColor.Red),
            <= 4 => ("Средний",  ConsoleColor.Yellow),
            _    => ("Надёжный", ConsoleColor.Green)
        };

        Console.Write("  Надёжность: ");
        ConsoleHelper.WriteLine(label, color);
        Console.WriteLine($"  Длина:      {password.Length} символов");
    }
}
