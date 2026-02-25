namespace ATMS.Admin.Service.Exceptions.Auth;

public enum AuthErrorType
{
    InvalidToken,
    InvalidRefreshToken,
    InvalidCredentials,
    PasswordMismatch,
    EmailNotConfirmed,
    EmailAlreadyConfirmed,
    TokenGenerationFailed,

    UserStatusLocked
}