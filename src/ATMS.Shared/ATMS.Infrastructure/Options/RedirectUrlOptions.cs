using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

public sealed class RedirectUrlOptions
{
    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ResetPasswordPage { get; init; }

    [Required]
    public required string EmailConfirmedPage { get; init; }

    [Required]
    public required string EmailAlreadyConfirmedPage { get; init; }

    [Required]
    public required string EmailConfirmFailedPage { get; init; }
}
