namespace PasswordManager.Services;

/// <summary>Параметры генерации пароля.</summary>
public sealed class PasswordGeneratorOptions
{
    public int  Length         { get; set; } = 16;
    public bool UseUppercase   { get; set; } = true;
    public bool UseLowercase   { get; set; } = true;
    public bool UseDigits      { get; set; } = true;
    public bool UseSpecial     { get; set; } = true;
}

/// <summary>Контракт генератора надёжных паролей.</summary>
public interface IPasswordGeneratorService
{
    /// <summary>
    /// Генерирует криптографически стойкий случайный пароль
    /// согласно переданным параметрам.
    /// </summary>
    string Generate(PasswordGeneratorOptions options);
}
