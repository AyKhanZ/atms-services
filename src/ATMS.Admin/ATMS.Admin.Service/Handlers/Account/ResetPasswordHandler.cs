using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public sealed class ResetPasswordHandler(
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IUniqueTokenService uniqueTokenService
    ) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = uniqueTokenService.Hash(command.Token);
        var entity = await passwordResetTokenRepository.FindAsync(
            t => t.TokenHash == tokenHash,
            cancellationToken);

        if (entity is null || entity.ExpiresAt < DateTime.UtcNow)
        {
            throw new AuthException(AuthErrorTypeEnum.InvalidToken,
                AccountMessages.InvalidPasswordResetToken);
        }

        var user = await userRepository.FindAsync(
            u => u.Id == entity.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);
        }

        user.PasswordHash = passwordHasherService.Hash(command.Password);
        var expectedVersion = user.SessionVersion;
        user.SessionVersion = expectedVersion + 1;

        // they proved the mailbox is theirs, so the timed lock is lifted (an admin lock stays)
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

        // password, used link and revoked sessions are saved together; a link used by a parallel reset fails here
        if (!await userRepository.TrySavePasswordChangeAsync(user, expectedVersion, DateTime.UtcNow, cancellationToken))
        {
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AccountMessages.InvalidPasswordResetToken);
        }
    }
}
