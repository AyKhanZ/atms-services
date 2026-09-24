using System.Globalization;
using System.Text;
using System.Text.Json;
using ATMS.Data.Enums;

namespace ATMS.Data.Criteria;

/// <summary>Where the next page starts: the last row's key and id, the direction, and which order they belong to.</summary>
public sealed record KeysetCursor(string Key, Guid Id, SortDirectionEnum SortDirection, string? Order = null)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static KeysetCursor For<TKey>(TKey key, Guid id, SortDirectionEnum sortDirection, string? order = null)
        => new(Write(key), id, sortDirection, order);

    public TKey KeyAs<TKey>() => Read<TKey>(Key);

    /// <summary>Whether the key reads as this type at all: a date that is not a date, for one, does not.</summary>
    public bool TryKeyAs<TKey>()
    {
        try
        {
            Read<TKey>(Key);
            return true;
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or NotSupportedException)
        {
            return false;
        }
    }

    public static bool TryDecode(string? value, out KeysetCursor? cursor)
    {
        cursor = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var padded = value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/')));
            cursor = JsonSerializer.Deserialize<KeysetCursor>(json, Options);
            return cursor is not null && cursor.Id != Guid.Empty && cursor.Key is not null;
        }
        catch
        {
            return false;
        }
    }

    public string Encode()
    {
        var json = JsonSerializer.Serialize(this, Options);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>An empty key means the row had no value there — a task without a deadline, for one.</summary>
    private static string Write<TKey>(TKey key) => key switch
    {
        null => string.Empty,
        DateTime date => date.ToString("O", CultureInfo.InvariantCulture),
        int number => number.ToString(CultureInfo.InvariantCulture),
        string text => text,
        _ => throw new NotSupportedException($"{typeof(TKey).Name} cannot be a keyset key.")
    };

    private static TKey Read<TKey>(string key)
    {
        var type = Nullable.GetUnderlyingType(typeof(TKey)) ?? typeof(TKey);

        if (key.Length == 0 && typeof(TKey) != typeof(string))
        {
            return default!;
        }

        object value = type switch
        {
            _ when type == typeof(DateTime) =>
                DateTime.Parse(key, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            _ when type == typeof(int) => int.Parse(key, CultureInfo.InvariantCulture),
            _ when type == typeof(string) => key,
            _ => throw new NotSupportedException($"{type.Name} cannot be a keyset key.")
        };

        return (TKey)value;
    }
}
