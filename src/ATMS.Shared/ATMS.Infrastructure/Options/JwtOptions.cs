namespace ATMS.Infrastructure.Options;

public class JwtOptions
{
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int TokenExpirationInMinutes { get; init; }
    public required int RefreshTokenExpirationInDays { get; init; }
    public required int EmailConfirmationTokenExpirationInHours { get; init; }
    public required int PasswordResetTokenExpirationInHours { get; init; }
    public required int MaxRefreshTokenLifetimeExpirationInDays { get; init; }
}
