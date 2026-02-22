namespace ATMS.Exceptions.Configuration;

public enum ConfigurationErrorType
{
    JWT_KeyNotFound,
    JWT_IssuerNotFound,
    JWT_AudienceNotFound,
    JWT_TokenExpirationInMinutesNotFound,
    JWT_RefreshTokenExpirationInDaysNotFound,
    JWT_EmailConfirmationTokenExpirationInHoursNotFound,
    JWT_MaxRefreshTokenLifetimeExpirationInDaysNotFound,

    Admin_EmailNotFound,
    Admin_PasswordNotFound,

    Email_FromNotFound,
    Email_SmtpServerNotFound,
    Email_PortNotFound,
    Email_UserNameNotFound,
    Email_PasswordNotFound,

    Images_BaseUrlNotFound,
    Images_RootPathNotFound,

    Queue_HostNotFound,
    Queue_UserNameNotFound,
    Queue_PasswordNotFound,

    RedirectUrl_BasePath,
    RedirectUrl_EmailConfirmedPage,
    RedirectUrl_ResetPasswordPage,

    Database_SqlConnectionNotFound,
    Database_MongoConnectionNotFound,
    Database_MongoDatabaseNotFound,


    JWT_SectionNotFound,
    Admin_SectionNotFound,
    Email_SectionNotFound,
    Images_SectionNotFound,
    Queue_SectionNotFound,
    RedirectUrl_SectionNotFound,
    Database_SectionNotFound,
}
