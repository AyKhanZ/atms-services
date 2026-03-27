namespace ATMS.Admin.Service.Exceptions.Auth;

public enum AuthErrorType
{
    InvalidToken,
    InvalidCredentials,
    EmailNotConfirmed,
    EmailAlreadyConfirmed,
    TokenGenerationFailed,
    AccountLocked,
    AccountInactive
}
