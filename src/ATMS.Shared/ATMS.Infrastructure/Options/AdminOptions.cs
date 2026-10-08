namespace ATMS.Infrastructure.Options;

public sealed class AdminOptions
{
    public required string Name { get; init; }

    public required string Surname { get; init; }

    public required string RoleName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}
