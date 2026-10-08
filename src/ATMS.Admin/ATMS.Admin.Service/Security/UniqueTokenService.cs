using ATMS.Admin.Service.Security.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Enums;

namespace ATMS.Admin.Service.Security;

public sealed class UniqueTokenService : IUniqueTokenService
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

        throw new AuthException(AuthErrorTypeEnum.TokenGenerationFailed, "Failed to generate a unique token.");
    }

    // only the hash goes to the db, so a leaked table gives no working tokens
    public string Hash(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(hash);
    }
}
