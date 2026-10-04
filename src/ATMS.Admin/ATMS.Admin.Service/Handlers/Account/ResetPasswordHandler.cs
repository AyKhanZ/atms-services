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
        var expectedVersion = user.SessionVersion;
        user.SessionVersion = expectedVersion + 1;

        // Whoever reset the password proved they own the mailbox, so the timed lockout from earlier
        // wrong guesses is lifted. A lock with no end date was set by hand and stays.
        user.FailedLoginCount = 0;
        if (user.UserStatusId == (int)UserStatusEnum.Locked && user.LockoutEnd.HasValue)
        {
            user.UserStatusId = (int)UserStatusEnum.Active;
        }

        user.LockoutEnd = null;

        passwordResetTokenRepository.StageConsume(entity);
        await passwordResetTokenRepository.ClearListAsync(
            prt => prt.UserId == entity.UserId,
            cancellationToken);

        // The new password, the used link and the revoked sessions commit together. Both the version
        // and the DELETE of this exact link must succeed, so a link used by a parallel reset is
        // rejected even when this request read the user only after that reset had committed.
        if (!await userRepository.TrySavePasswordChangeAsync(user, expectedVersion, DateTime.UtcNow, cancellationToken))
        {
            throw new AuthException(AuthErrorType.InvalidToken, AccountMessages.InvalidPasswordResetToken);
        }
    }
}
