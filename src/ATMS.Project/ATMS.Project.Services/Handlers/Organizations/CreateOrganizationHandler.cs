using ATMS.Infrastructure.Enums;
using ATMS.Infrastructure.Images;
using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public sealed class CreateOrganizationHandler(
    IMapper mapper,
    IImageStorage imageStorage,
    IOrganizationRepository organizationRepository)
    : IRequestHandler<CreateOrganizationCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Organization>(command);
        entity.Id = Guid.NewGuid();

        if (command.Logo is not null)
        {
            var storedImage = await imageStorage.SaveAsync(
                command.Logo,
                ImageStorageFolderEnum.Organizations,
                entity.Id,
                cancellationToken);

            entity.LogoPath = storedImage.RelativePath;
        }
        
        await organizationRepository.CreateAsync(entity, cancellationToken);

        return entity.Id;
    }
}
