using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IEmailDeliveryRepository emailDeliveryRepository,
    EmailDeliveryRequestLock emailDeliveryRequestLock) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await emailDeliveryRequestLock.ExecuteAsync(async () =>
        {
            var user = await userRepository.FindAsync(u => u.Email == command.Email, cancellationToken);
            if (user is null)
            {
                throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
            }

            await emailDeliveryRepository.RemoveUnsentAsync(
                user.Id,
                EmailDeliveryTypeEnum.PasswordReset,
                cancellationToken);
            await emailDeliveryRepository.AddPasswordResetAsync(user.Id, cancellationToken);
            await userRepository.SaveAsync(cancellationToken);
        }, cancellationToken);
    }
}
