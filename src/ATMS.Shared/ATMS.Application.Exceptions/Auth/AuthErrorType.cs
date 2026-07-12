namespace ATMS.Application.Exceptions.Auth;

public enum AuthErrorType
{
    InvalidToken,
    InvalidCredentials,
    EmailNotConfirmed,
    EmailAlreadyConfirmed,
    TokenGenerationFailed,
    AccountLocked,
    Forbidden,
    AccountInactive
}
