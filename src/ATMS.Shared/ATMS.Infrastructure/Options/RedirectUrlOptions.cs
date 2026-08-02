namespace ATMS.Infrastructure.Options;

public class RedirectUrlOptions
{
    public required string BaseUrl { get; init; }
    public required string ResetPasswordPage { get; init; }
    public required string EmailConfirmedPage { get; init; }
    public required string EmailAlreadyConfirmedPage { get; init; }
    public required string EmailConfirmFailedPage { get; init; }
}
