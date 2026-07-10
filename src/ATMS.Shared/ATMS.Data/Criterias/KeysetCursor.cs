using System.Text;
using System.Text.Json;
using ATMS.Data.Enums;

namespace ATMS.Data.Criterias;

public sealed record KeysetCursor(DateTime CreatedAt, Guid Id, SortDirectionEnum SortDirection)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

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
            return cursor is not null && cursor.Id != Guid.Empty;
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
}
