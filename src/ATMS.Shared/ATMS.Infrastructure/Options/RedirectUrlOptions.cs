namespace ATMS.Infrastructure.Options;

public class RedirectUrlOptions
{
    public required string BasePath { get; init; }
    public required string ResetPasswordPage { get; init; }
    public required string EmailConfirmedPage { get; init; }
}
