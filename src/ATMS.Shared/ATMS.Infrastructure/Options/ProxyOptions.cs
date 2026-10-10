namespace ATMS.Infrastructure.Options;

public sealed class ProxyOptions
{
    // no default: the binder appends to an initialized array instead of replacing it
    public string[]? KnownNetworks { get; init; }
}
