namespace ATMS.Admin.Service.Security.Models;

public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAt,
    DateTime FamilyExpiresAt);
