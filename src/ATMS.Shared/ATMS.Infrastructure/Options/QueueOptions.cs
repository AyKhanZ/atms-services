namespace ATMS.Infrastructure.Options;

public class QueueOptions
{
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required int Port { get; init; }
    public required string VirtualHost { get; init; }
}
