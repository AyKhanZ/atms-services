using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Security;

public class ResetPasswordTokenService(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUniqueTokenService uniqueTokenService,
    IConfiguration configuration) : IResetPasswordTokenService
{
    private readonly JwtOptions _jwtOptions =
        configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                string.Format(LogMessages.ConfigSectionNotFound, nameof(JwtOptions)));

    public async Task<ResetPasswordTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        await passwordResetTokenRepository.ClearListAsync(
            x => x.UserId == user.Id,
            cancellationToken);

        var resetPasswordToken = await uniqueTokenService.GenerateUniqueAsync(
            async token => await passwordResetTokenRepository.IsExistAsync(token, cancellationToken)
            );

        var expiresAt = DateTime.UtcNow.AddHours(_jwtOptions.PasswordResetTokenExpirationInHours);

        await passwordResetTokenRepository.AddToListAsync(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Token = resetPasswordToken,
            UserId = user.Id,
            ExpiresAt = expiresAt,
        }, cancellationToken);

        return new ResetPasswordTokenResult(resetPasswordToken, expiresAt);
    }
}
