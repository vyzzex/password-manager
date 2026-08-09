using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services;

/// <summary>
/// Генератор паролей на базе <see cref="RandomNumberGenerator"/> —
/// криптографически стойкого источника случайности.
///
/// Алгоритм:
///   1. Формируем алфавит из разрешённых символов.
///   2. Гарантируем наличие минимум одного символа из каждой выбранной группы.
///   3. Заполняем оставшиеся позиции случайными символами из общего алфавита.
///   4. Перемешиваем результат алгоритмом Fisher-Yates (с крипто-RNG).
/// </summary>
public sealed class PasswordGeneratorService : IPasswordGeneratorService
{
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits    = "0123456789";
    private const string Special   = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    /// <inheritdoc/>
    public string Generate(PasswordGeneratorOptions options)
    {
        if (options.Length < 4)
            throw new ArgumentException("Длина пароля должна быть не менее 4 символов.", nameof(options));

        var alphabet = BuildAlphabet(options);

        if (alphabet.Length == 0)
            throw new ArgumentException("Выберите хотя бы одну группу символов.");

        var passwordChars = new List<char>(options.Length);

        // Гарантируем минимум по одному символу из каждой группы
        if (options.UseUppercase) passwordChars.Add(GetRandom(Uppercase));
        if (options.UseLowercase) passwordChars.Add(GetRandom(Lowercase));
        if (options.UseDigits)    passwordChars.Add(GetRandom(Digits));
        if (options.UseSpecial)   passwordChars.Add(GetRandom(Special));

        // Заполняем оставшиеся позиции
        while (passwordChars.Count < options.Length)
            passwordChars.Add(GetRandom(alphabet));

        // Перемешиваем Fisher-Yates с крипто-RNG
        Shuffle(passwordChars);

        return new string(passwordChars.ToArray());
    }

    // Формирует строку допустимых символов
    private static string BuildAlphabet(PasswordGeneratorOptions options)
    {
        var sb = new StringBuilder();
        if (options.UseUppercase) sb.Append(Uppercase);
        if (options.UseLowercase) sb.Append(Lowercase);
        if (options.UseDigits)    sb.Append(Digits);
        if (options.UseSpecial)   sb.Append(Special);
        return sb.ToString();
    }

    // Возвращает случайный символ из строки
    private static char GetRandom(string source)
    {
        var index = RandomNumberGenerator.GetInt32(source.Length);
        return source[index];
    }

    // Перемешивание Fisher-Yates
    private static void Shuffle(List<char> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
