namespace ATMS.Admin.Service.Exceptions.Auth;

public enum AuthErrorType
{
    InvalidToken,
    InvalidCredentials,
    PasswordMismatch,
    EmailNotConfirmed,
    EmailAlreadyConfirmed,
    TokenGenerationFailed,

    UserStatusLocked
}