using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ATMS.Admin.Service.Handlers.Account;

public sealed class ConfirmEmailHandler(
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IUserRepository userRepository,
    ICacheService cache) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResultEnum>
{
    public async Task<ConfirmEmailResultEnum> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var principal = await emailConfirmationTokenService.ValidateTokenAsync(command.Token);
        if (principal == null)
        {
            return ConfirmEmailResultEnum.Failed;
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ConfirmEmailResultEnum.Failed;
        }

        var user = await userRepository.FindAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return ConfirmEmailResultEnum.Failed;
        }

        if (user.EmailConfirmed)
        {
            return ConfirmEmailResultEnum.AlreadyConfirmed;
        }

        user.EmailConfirmed = true;
        await userRepository.SaveAsync(cancellationToken);

        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(user.Id, language), cancellationToken);
        }
        
        return ConfirmEmailResultEnum.Confirmed;
    }
}
