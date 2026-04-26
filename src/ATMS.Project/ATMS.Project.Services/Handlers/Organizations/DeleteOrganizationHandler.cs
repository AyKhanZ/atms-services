using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class DeleteOrganizationHandler(IOrganizationRepository organizationRepository,ICurrentUser currentUser)
    : IRequestHandler<DeleteOrganizationCommand>
{
    public async Task Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
    {
        var entity = await organizationRepository
            .FindAsync(o => o.Id == command.Id, cancellationToken);

        if (entity is null)
        {
            throw new EntityException(EntityErrorType.NotFound, OrganizationMessages.NotFound);
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedById = currentUser.Id;

        await organizationRepository.SaveAsync(cancellationToken);
    }
}