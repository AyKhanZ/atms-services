using System.Globalization;

namespace ATMS.Email.Resources;

public static class EmailGreeting
{
    public static string For(string? name, string? surname)
    {
        var fullName = $"{name} {surname}".Trim();
        return string.IsNullOrWhiteSpace(fullName)
            ? EmailMessages.GreetingNoName
            : string.Format(CultureInfo.CurrentCulture, EmailMessages.Greeting, fullName);
    }
}
