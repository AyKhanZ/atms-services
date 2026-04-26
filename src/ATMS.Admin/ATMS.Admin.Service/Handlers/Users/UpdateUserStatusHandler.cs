using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class UpdateUserStatusHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserStatusCommand>
{
    public async Task Handle(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        entity.UserStatusId = command.UserStatusId;
        
        await userRepository.SaveAsync(cancellationToken);
    }
}