using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class ResetPasswordHandler(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUserSessionRepository userSessionRepository
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

        // Whoever reset the password proved they own the mailbox: a lockout from earlier wrong
        // guesses would only keep them out with the new password for another 15 minutes.
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        if (user.UserStatusId == (int)UserStatusEnum.Locked)
        {
            user.UserStatusId = (int)UserStatusEnum.Active;
        }

        await passwordResetTokenRepository.ClearListAsync(
            prt => prt.UserId == entity.UserId,
            cancellationToken);

        // Staged, not written: the new password, the used token and the revoked sessions commit
        // together, so a failure cannot leave old sessions alive with the reset link spent.
        await userSessionRepository.StageRevokeAllAsync(user.Id, DateTime.UtcNow, cancellationToken);

        await userRepository.SaveAsync(cancellationToken);
    }
}
