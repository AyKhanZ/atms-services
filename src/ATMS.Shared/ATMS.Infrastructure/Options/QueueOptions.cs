using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class QueueOptions
{
    [Required]
    public required string Host { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    public required string Password { get; init; }

    [Range(1, int.MaxValue)]
    public required int Port { get; init; }

    [Required]
    public required string VirtualHost { get; init; }
}
