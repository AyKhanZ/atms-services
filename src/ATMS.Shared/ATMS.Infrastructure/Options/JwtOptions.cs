using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class JwtOptions
{
    [Required]
    public required string Key { get; init; }

    [Required]
    public required string Issuer { get; init; }

    [Required]
    public required string Audience { get; init; }

    [Range(1, int.MaxValue)]
    public required int TokenExpirationInMinutes { get; init; }

    [Range(1, int.MaxValue)]
    public required int RefreshTokenExpirationInDays { get; init; }

    [Range(1, int.MaxValue)]
    public required int EmailConfirmationTokenExpirationInHours { get; init; }

    [Range(1, int.MaxValue)]
    public required int PasswordResetTokenExpirationInHours { get; init; }

    [Range(1, int.MaxValue)]
    public required int MaxRefreshTokenLifetimeExpirationInDays { get; init; }
}
