using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public sealed class ResendEmailConfirmationHandler(
    IUserRepository userRepository,
    IPasswordHasherService passwordHasherService,
    IPasswordService passwordService,
    IEmailDeliveryRepository emailDeliveryRepository,
    EmailDeliveryRequestLock emailDeliveryRequestLock
    ) : IRequestHandler<ResendEmailConfirmationCommand>
{
    private static readonly TimeSpan EmailPause = TimeSpan.FromMinutes(2);

    public async Task Handle(ResendEmailConfirmationCommand command, CancellationToken cancellationToken)
    {
        await emailDeliveryRequestLock.ExecuteAsync(async () =>
        {
            var user = await userRepository.FindAsync(
                u => u.Email == command.Email,
                cancellationToken);

            if (user is null)
            {
                throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);
            }

            if (user.EmailConfirmed)
            {
                throw new AuthException(AuthErrorTypeEnum.EmailAlreadyConfirmed,
                    AccountMessages.EmailAlreadyConfirmed);
            }

            // a letter a moment ago: leave the password as it is
            if (await emailDeliveryRepository.AnySinceAsync(
                    user.Id,
                    EmailDeliveryTypeEnum.Confirmation,
                    DateTime.UtcNow - EmailPause,
                    cancellationToken))
            {
                return;
            }

            var rndPassword = passwordService.GenerateRandomPassword();
            user.PasswordHash = passwordHasherService.Hash(rndPassword);

            await emailDeliveryRepository.RemoveUnsentAsync(
                user.Id,
                EmailDeliveryTypeEnum.Confirmation,
                cancellationToken);

            await emailDeliveryRepository.AddConfirmationAsync(
                user.Id,
                rndPassword,
                cancellationToken);

            await userRepository.SaveAsync(cancellationToken);
        }, cancellationToken);
    }
}
