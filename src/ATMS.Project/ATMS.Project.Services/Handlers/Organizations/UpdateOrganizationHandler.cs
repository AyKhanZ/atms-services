using ATMS.Application.Exceptions.Entity;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class UpdateOrganizationHandler(IOrganizationRepository organizationRepository)
    : IRequestHandler<UpdateOrganizationCommand>
{
    public async Task Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var entity = await organizationRepository
            .FindAsync(o => o.Id == command.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityException(EntityErrorType.NotFound, OrganizationMessages.NotFound);
        }
        
        entity.Title = command.Title;
        entity.Voen = command.Voen;
        if (command.LogoPath != null)
        {
            entity.LogoPath = command.LogoPath;  
        }
        
        await organizationRepository.SaveAsync(cancellationToken);
    }
}