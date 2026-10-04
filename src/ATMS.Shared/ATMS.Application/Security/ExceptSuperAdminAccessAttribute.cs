namespace ATMS.Application.Security;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExceptSuperAdminAccessAttribute : Attribute;
