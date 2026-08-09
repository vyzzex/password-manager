namespace PasswordManager.UI;

/// <summary>
/// Вспомогательные методы для работы с консолью.
/// Инкапсулирует логику безопасного ввода и форматирования вывода.
/// </summary>
public static class ConsoleHelper
{
    // Цвета для удобства восприятия
    public static readonly ConsoleColor AccentColor  = ConsoleColor.Cyan;
    public static readonly ConsoleColor SuccessColor = ConsoleColor.Green;
    public static readonly ConsoleColor ErrorColor   = ConsoleColor.Red;
    public static readonly ConsoleColor WarnColor    = ConsoleColor.Yellow;

    /// <summary>
    /// Считывает пароль из консоли, маскируя каждый символ звёздочкой.
    /// Поддерживает удаление символа по Backspace.
    /// </summary>
    public static string ReadPassword(string prompt = "Пароль: ")
    {
        Write(prompt, WarnColor);

        var password = new System.Text.StringBuilder();

        ConsoleKeyInfo key;
        while (true)
        {
            key = Console.ReadKey(intercept: true); // intercept: true — не выводить символ

            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    // Визуально удаляем последнюю звёздочку
                    Console.Write("\b \b");
                }
                continue;
            }

            // Игнорируем управляющие символы
            if (char.IsControl(key.KeyChar))
                continue;

            password.Append(key.KeyChar);
            Console.Write('*');
        }

        Console.WriteLine(); // Перенос строки после ввода пароля
        return password.ToString();
    }

    /// <summary>Выводит текст с указанным цветом, затем сбрасывает цвет.</summary>
    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    /// <summary>Выводит текст с переносом строки и указанным цветом.</summary>
    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    /// <summary>Выводит сообщение об ошибке.</summary>
    public static void WriteError(string message)
        => WriteLine($"[ОШИБКА] {message}", ErrorColor);

    /// <summary>Выводит сообщение об успехе.</summary>
    public static void WriteSuccess(string message)
        => WriteLine($"[OK] {message}", SuccessColor);

    /// <summary>Выводит предупреждение.</summary>
    public static void WriteWarning(string message)
        => WriteLine($"[!] {message}", WarnColor);

    /// <summary>Выводит горизонтальный разделитель.</summary>
    public static void WriteSeparator(char ch = '─', int width = 60)
        => WriteLine(new string(ch, width), ConsoleColor.DarkGray);

    /// <summary>Выводит заголовок раздела.</summary>
    public static void WriteHeader(string title)
    {
        WriteSeparator();
        WriteLine($"  {title}", AccentColor);
        WriteSeparator();
    }

    /// <summary>Просит пользователя нажать Enter, чтобы продолжить.</summary>
    public static void PressEnterToContinue()
    {
        WriteLine("\nНажмите Enter для продолжения...", ConsoleColor.DarkGray);
        Console.ReadLine();
    }

    /// <summary>
    /// Запрашивает строку с валидацией — повторяет запрос, пока строка не пустая.
    /// </summary>
    public static string ReadRequiredLine(string prompt)
    {
        string? value;
        do
        {
            Console.Write(prompt);
            value = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(value))
                WriteWarning("Поле не может быть пустым. Попробуйте ещё раз.");
        } while (string.IsNullOrEmpty(value));

        return value;
    }

    /// <summary>Запрашивает целое число в диапазоне [min, max].</summary>
    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out var result) && result >= min && result <= max)
                return result;

            WriteWarning($"Введите число от {min} до {max}.");
        }
    }

    /// <summary>Запрашивает ответ да/нет.</summary>
    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (д/н): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (input is "д" or "y" or "да" or "yes") return true;
            if (input is "н" or "n" or "нет" or "no") return false;
            WriteWarning("Введите 'д' или 'н'.");
        }
    }
}
