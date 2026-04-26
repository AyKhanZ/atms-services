using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class CreateOrganizationHandler(
    IMapper mapper, IOrganizationRepository organizationRepository)
    : IRequestHandler<CreateOrganizationCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Organization>(command);
        entity.Id = Guid.NewGuid();
        
        await organizationRepository.CreateAsync(entity, cancellationToken);

        return entity.Id;
    }
}
