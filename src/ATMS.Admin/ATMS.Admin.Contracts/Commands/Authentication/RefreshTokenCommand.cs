namespace ATMS.Admin.Contracts.Commands.Authentication;

public class RefreshTokenCommand
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
}
