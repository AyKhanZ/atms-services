using System.Text.RegularExpressions;

namespace ATMS.Application.Dispatcher.Validation;

public static class PasswordHelper
{
    private const string SpecialCharacters = "!@#$%^&*()-_+=";

    // Allows only Latin letters, digits and the special characters accepted by our password policy.
    private static readonly Regex AllowedCharactersPattern = new(
        @"^[A-Za-z\d!@#$%^&*()\-_+=]+$",
        RegexOptions.Compiled);

    public static bool IsValid(string password, int minimumLength, bool requireLowercase)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            password.Length < minimumLength ||
            password.Length > 40 ||
            !AllowedCharactersPattern.IsMatch(password))
        {
            return false;
        }

        // A valid password always has an uppercase letter, digit and special character.
        // Some flows, such as onboarding, also require a lowercase letter.
        return password.Any(char.IsUpper) &&
               (!requireLowercase || password.Any(char.IsLower)) &&
               password.Any(char.IsDigit) &&
               password.Any(SpecialCharacters.Contains);
    }
}
