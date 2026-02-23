namespace ATMS.Admin.Service.Security.Models;

public sealed record TokenResult(string AccessToken, DateTime ExpiresInMinutes);