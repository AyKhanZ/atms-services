namespace ATMS.Admin.API.Middleware.Exceptions.Auth;

public enum AuthErrorType
{
    InvalidToken,
    InvalidRefreshToken,
    InvalidCredentials,
    PasswordMismatch,
    EmailNotConfirmed,
    EmailAlreadyConfirmed
}