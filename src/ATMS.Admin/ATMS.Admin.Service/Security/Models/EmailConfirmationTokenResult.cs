namespace ATMS.Admin.Service.Security.Models;

public sealed record EmailConfirmationTokenResult(string Token, DateTime ExpiresInHours);
