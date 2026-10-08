using ATMS.Project.Services.Domain.Board.Interfaces;

namespace ATMS.Project.Services.Domain.Board;

// the position is a text key with room between any two neighbours, so a move rewrites one row
public sealed class WorkTaskBoardPositionService : IWorkTaskBoardPositionService
{
    private const string Digits = "0123456789abcdefghijklmnopqrstuvwxyz";
    private const int HeadLength = 12;

    public int MaxLength => 64;

    public string Between(string? above, string? below)
    {
        var low = above ?? string.Empty;

        if (below is not null && string.CompareOrdinal(low, below) >= 0)
        {
            throw new ArgumentException($"'{above}' must sort before '{below}'.");
        }

        var key = above is null ? BeforeTheFirst(below) : Midpoint(low, below);

        if (key.Length > MaxLength)
        {
            throw new InvalidOperationException("The interval between the two cards is used up.");
        }

        return key;
    }

    // on top: step back a whole digit instead of halving, keeps the keys short
    private static string BeforeTheFirst(string? below)
    {
        if (below is null)
        {
            return "hzzzzzzzzzzzv";
        }

        var digits = below.PadRight(HeadLength, Digits[0])[..HeadLength].ToCharArray();

        for (var index = digits.Length - 1; index >= 0; index--)
        {
            var digit = Digits.IndexOf(digits[index]);

            if (digit > 0)
            {
                digits[index] = Digits[digit - 1];

                return new string(digits) + "v";
            }

            digits[index] = Digits[^1];
        }

        return Midpoint(string.Empty, below);
    }

    private static string Midpoint(string low, string? high)
    {
        if (high is not null)
        {
            var shared = 0;
            while (shared < high.Length && (shared < low.Length ? low[shared] : Digits[0]) == high[shared])
            {
                shared++;
            }

            if (shared > 0)
            {
                return high[..shared] + Midpoint(Rest(low, shared), high[shared..]);
            }
        }

        var lowDigit = low.Length > 0 ? Digits.IndexOf(low[0]) : 0;
        var highDigit = high is not null ? Digits.IndexOf(high[0]) : Digits.Length;

        if (highDigit - lowDigit > 1)
        {
            return Digits[(lowDigit + highDigit + 1) / 2].ToString();
        }

        return high is { Length: > 1 }
            ? high[..1]
            : Digits[lowDigit] + Midpoint(Rest(low, 1), null);
    }

    private static string Rest(string value, int start) => start < value.Length ? value[start..] : string.Empty;
}
