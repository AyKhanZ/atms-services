namespace ATMS.Application.Exceptions.Enums;

public enum AuthErrorTypeEnum
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
