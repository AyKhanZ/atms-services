using System.Security.Claims;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class ConfirmEmailHandler(
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IUserRepository userRepository) : IRequestHandler<ConfirmEmailCommand,bool>
{
    public async Task<bool> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var principal = await emailConfirmationTokenService.ValidateTokenAsync(command.Token);
        if (principal == null)
            return false;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return false;

        var user = await userRepository.GetAsync(userId, cancellationToken);
        if (user == null)
            return false;

        if (user.EmailConfirmed)
            return true;

        user.EmailConfirmed = true;
        await userRepository.SaveAsync(cancellationToken);
        
        return true;
    }
}
