using ATMS.Project.Services.Board.Interfaces;

namespace ATMS.Project.Services.Board;

/// <summary>
/// Where a card sits in its column, as a key that sorts as text. Two neighbours always have room
/// between them, so moving one card rewrites one row instead of renumbering the column.
/// </summary>
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

    /// <summary>
    /// A card put on top of the column. It steps back by a whole digit instead of halving the gap,
    /// so adding a hundred cards in a row keeps the keys short.
    /// </summary>
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

    /// <summary>A key strictly between the two, digit by digit — the halfway point of the interval.</summary>
    private static string Midpoint(string low, string? high)
    {
        if (high is not null)
        {
            // What both keys start with stays as it is; the new key differs only after it.
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

        // The digits are neighbours: the longer key already reaches past the shorter one.
        return high is { Length: > 1 }
            ? high[..1]
            : Digits[lowDigit] + Midpoint(Rest(low, 1), null);
    }

    private static string Rest(string value, int start) => start < value.Length ? value[start..] : string.Empty;
}
