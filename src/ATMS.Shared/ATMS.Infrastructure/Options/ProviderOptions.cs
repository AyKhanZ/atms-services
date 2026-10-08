namespace ATMS.Infrastructure.Options;

public sealed class ProviderOptions
{
    public required string AdminServiceUrl { get; init; }

    public required string ProjectServiceUrl { get; init; }

    public required int TimeoutSeconds { get; init; } 
}