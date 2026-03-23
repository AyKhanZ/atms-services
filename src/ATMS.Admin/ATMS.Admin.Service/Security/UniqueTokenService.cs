using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace ATMS.Admin.Service.Security;

public class UniqueTokenService : IUniqueTokenService
{
    private static string Generate(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public async Task<string> GenerateUniqueAsync(
        Func<string, Task<bool>> existsAsync,
        int maxAttempts = 5)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            var token = Generate();

            if (!await existsAsync(token))
                return token;
        }

        throw new AuthException(AuthErrorType.TokenGenerationFailed, "Failed to generate a unique token.");
    }
}
