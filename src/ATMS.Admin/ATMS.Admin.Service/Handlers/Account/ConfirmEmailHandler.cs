using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ATMS.Admin.Service.Handlers.Account;

public class ConfirmEmailHandler(
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IUserRepository userRepository) : IRequestHandler<ConfirmEmailCommand,bool>
{
    public async Task<bool> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var principal = await emailConfirmationTokenService.ValidateTokenAsync(command.Token);
        if (principal == null)
        {
            return false;
        }

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        var user = await userRepository.FindAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return false;
        }
        if (user.EmailConfirmed)
        {
            throw new AuthException(AuthErrorType.EmailAlreadyConfirmed,
                AccountMessages.EmailAlreadyConfirmed);
        }

        user.EmailConfirmed = true;
        await userRepository.SaveAsync(cancellationToken);
        
        return true;
    }
}
