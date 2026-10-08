namespace ATMS.Infrastructure.Options;

public sealed class RedisOptions
{
    public required string ConnectionString { get; init; }

    public required string InstanceName { get; init; }
}