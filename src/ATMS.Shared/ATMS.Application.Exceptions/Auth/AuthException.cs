using ATMS.Application.Exceptions.Enums;

namespace ATMS.Application.Exceptions.Auth;

public sealed class AuthException : Exception
{
    public AuthErrorTypeEnum AuthErrorType { get; set; }
    public AuthException(AuthErrorTypeEnum authErrorType, string message)
        : base(message) => AuthErrorType = authErrorType;
}
