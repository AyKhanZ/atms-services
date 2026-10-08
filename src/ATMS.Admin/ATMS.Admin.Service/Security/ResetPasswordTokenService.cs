using Microsoft.Extensions.Options;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;

namespace ATMS.Admin.Service.Security;

public sealed class ResetPasswordTokenService(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUniqueTokenService uniqueTokenService,
    IOptions<JwtOptions> jwtOptions) : IResetPasswordTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<ResetPasswordTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        await passwordResetTokenRepository.ClearListAsync(
            x => x.UserId == user.Id,
            cancellationToken);

        var resetPasswordToken = await uniqueTokenService.GenerateUniqueAsync(
            token => passwordResetTokenRepository.IsTokenHashExistsAsync(uniqueTokenService.Hash(token), cancellationToken));

        var expiresAt = DateTime.UtcNow.AddHours(_jwtOptions.PasswordResetTokenExpirationInHours);

        await passwordResetTokenRepository.AddToListAsync(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            TokenHash = uniqueTokenService.Hash(resetPasswordToken),
            UserId = user.Id,
            ExpiresAt = expiresAt,
        }, cancellationToken);

        return new ResetPasswordTokenResult(resetPasswordToken, expiresAt);
    }
}
