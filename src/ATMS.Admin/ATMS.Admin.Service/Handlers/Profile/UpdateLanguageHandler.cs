using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public class UpdateLanguageHandler(IUserRepository userRepository) : IRequestHandler<UpdateLanguageCommand>
{
    public async Task Handle(UpdateLanguageCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        entity.Language = command.Language;
        
        await userRepository.SaveAsync(cancellationToken);
    }
}