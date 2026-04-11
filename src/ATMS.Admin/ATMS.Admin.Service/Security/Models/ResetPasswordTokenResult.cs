namespace ATMS.Admin.Service.Security.Models;

public sealed record ResetPasswordTokenResult(string Token, DateTime ExpiresInHours);
