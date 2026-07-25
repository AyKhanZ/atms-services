using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ATMS.Admin.Service.Handlers.Account;

public class ConfirmEmailHandler(
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IUserRepository userRepository) : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResultEnum>
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
        
        return ConfirmEmailResultEnum.Confirmed;
    }
}
