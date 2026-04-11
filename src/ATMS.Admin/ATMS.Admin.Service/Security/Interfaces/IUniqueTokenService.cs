namespace ATMS.Admin.Service.Security.Interfaces;

public interface IUniqueTokenService
{
    Task<string> GenerateUniqueAsync(
        Func<string, Task<bool>> existsAsync,
        int maxAttempts = 5);
}
