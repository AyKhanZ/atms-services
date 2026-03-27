using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class ResetPasswordHandler(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService
    ) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var entity = await passwordResetTokenRepository.FindAsync(
            t => t.Token == command.Token,
            cancellationToken);

        if (entity is null || entity.ExpiresAt < DateTime.UtcNow)
        {
            throw new AuthException(AuthErrorType.InvalidToken,
                AccountMessages.InvalidPasswordResetToken);
        }

        var user = await userRepository.FindAsync(
            u => u.Id == entity.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }

        user.PasswordHash = passwordHasherService.Hash(command.Password);

        await passwordResetTokenRepository.ClearListAsync(
            prt => prt.UserId == entity.UserId,
            cancellationToken);

        await userRepository.SaveAsync(cancellationToken);
    }
}
