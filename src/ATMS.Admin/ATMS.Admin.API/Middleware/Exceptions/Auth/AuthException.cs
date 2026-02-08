namespace ATMS.Admin.API.Middleware.Exceptions.Auth;

public class AuthException : Exception
{
    public AuthErrorType AuthErrorType { get; set; }
    public AuthException(AuthErrorType authErrorType, string message) : base(message)
    {
        AuthErrorType = authErrorType;
    }
}
