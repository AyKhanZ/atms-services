namespace ATMS.Application.Exceptions.Enums;

public enum ConfigurationErrorTypeEnum
{
    JwtSectionNotFound,
    EmailSectionNotFound,
    ProviderSectionNotFound,
    DatabaseSectionNotFound,
    MissingSeedData,
    BusinessTimeZoneNotFound,
    BusinessTimeZoneUnavailable
}
