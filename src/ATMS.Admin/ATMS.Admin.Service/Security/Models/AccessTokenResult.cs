namespace ATMS.Admin.Service.Security.Models;

public sealed record AccessTokenResult(string Token, DateTime ExpiresInMinutes);
