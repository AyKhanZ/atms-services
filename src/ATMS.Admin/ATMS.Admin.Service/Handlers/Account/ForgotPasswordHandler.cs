using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public sealed class ForgotPasswordHandler(
    IUserRepository userRepository,
    IEmailDeliveryRepository emailDeliveryRepository,
    EmailDeliveryRequestLock emailDeliveryRequestLock) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await emailDeliveryRequestLock.ExecuteAsync(async () =>
        {
            // search by normalized email, otherwise a different case silently sends nothing
            var normalizedEmail = command.Email.Trim().ToUpperInvariant();
            var user = await userRepository.FindAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            // same answer for unknown emails, so nobody can check who has an account
            if (user is null)
            {
                return;
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
