using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public class UpdatePhotoHandler(IUserRepository userRepository) : IRequestHandler<UpdatePhotoCommand>
{
    public async Task Handle(UpdatePhotoCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        entity.AvatarPath = command.FileName;
        
        await userRepository.SaveAsync(cancellationToken);
    }
}