using System.Globalization;
using System.Text;
using System.Text.Json;
using ATMS.Data.Enums;

namespace ATMS.Data.Criteria;

public sealed record KeysetCursor(string Key, Guid Id, SortDirectionEnum SortDirection)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static KeysetCursor For<TKey>(TKey key, Guid id, SortDirectionEnum sortDirection)
        => new(Write(key), id, sortDirection);

    public TKey KeyAs<TKey>() => Read<TKey>(Key);

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
