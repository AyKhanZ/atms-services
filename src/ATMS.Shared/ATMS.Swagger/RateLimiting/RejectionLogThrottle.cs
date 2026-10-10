using System.Collections.Concurrent;

namespace ATMS.Swagger.RateLimiting;

// one log line per policy and key a minute
internal sealed class RejectionLogThrottle
{
    private const long IntervalMs = 60_000;
    private const int MaxEntries = 10_000;

    private readonly ConcurrentDictionary<string, long> lastLogged = new();

    public bool ShouldLog(string policy, string key)
    {
        var now = Environment.TickCount64;
        var entry = $"{policy}|{key}";
        if (lastLogged.TryGetValue(entry, out var last) && now - last < IntervalMs)
        {
            return false;
        }

        // keys are per user or IP, a flood of new IPs must not grow this forever
        if (lastLogged.Count >= MaxEntries)
        {
            lastLogged.Clear();
        }

        lastLogged[entry] = now;
        return true;
    }
}
