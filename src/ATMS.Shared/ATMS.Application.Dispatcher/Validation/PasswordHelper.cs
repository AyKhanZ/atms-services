using System.Text.RegularExpressions;

namespace ATMS.Application.Dispatcher.Validation;

public static class PasswordHelper
{
    private const string SpecialCharacters = "!@#$%^&*()-_+=";

    // latin letters, digits and the allowed special chars only
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

        // upper + digit + special always, lowercase only in some flows (onboarding)
        return password.Any(char.IsUpper) &&
               (!requireLowercase || password.Any(char.IsLower)) &&
               password.Any(char.IsDigit) &&
               password.Any(SpecialCharacters.Contains);
    }
}
