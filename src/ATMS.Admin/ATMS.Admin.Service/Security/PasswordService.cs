using ATMS.Admin.Service.Security.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ATMS.Admin.Service.Security;

public class PasswordService : IPasswordService
{
    private const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+";
    private const int PasswordLength = 10;

    private static readonly string AllValidChars =
        UpperCaseChars + LowerCaseChars + Digits + SpecialChars;

    public string GenerateRandomPassword()
    {
        var password = new StringBuilder();

        password.Append(GetRandomChar(UpperCaseChars));
        password.Append(GetRandomChar(LowerCaseChars));
        password.Append(GetRandomChar(Digits));
        password.Append(GetRandomChar(SpecialChars));

        while (password.Length < PasswordLength)
        {
            password.Append(GetRandomChar(AllValidChars));
        }

        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        int index = RandomNumberGenerator.GetInt32(chars.Length);
        return chars[index];
    }

    private static string Shuffle(string input)
    {
        var array = input.ToCharArray();

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return new string(array);
    }
}
