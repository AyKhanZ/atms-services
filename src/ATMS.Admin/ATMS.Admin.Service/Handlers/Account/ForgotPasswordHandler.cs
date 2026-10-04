using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Delivery;
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
            // By the normalized address: with the same answer for every email, a case mismatch would
            // otherwise fail silently and no letter would come.
            var normalizedEmail = command.Email.Trim().ToUpperInvariant();
            var user = await userRepository.FindAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            // The answer is the same whether or not the address is registered, so the form cannot be
            // used to find out who has an account.
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
