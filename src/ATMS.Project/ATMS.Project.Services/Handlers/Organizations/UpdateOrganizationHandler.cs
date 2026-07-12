using ATMS.Application.Exceptions.Entity;
using ATMS.Infrastructure.Images;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Organizations;

public class UpdateOrganizationHandler(
    IImageStorage imageStorage,
    IOrganizationRepository organizationRepository)
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

        var oldLogoPath = entity.LogoPath;
        entity.Title = command.Title;
        entity.Voen = command.Voen;
        entity.UpdatedAt = DateTime.UtcNow;

        if (command.Logo is not null)
        {
            var storedImage = await imageStorage.SaveAsync(
                command.Logo,
                ImageStorageFolder.Organizations,
                entity.Id,
                cancellationToken);

            entity.LogoPath = storedImage.RelativePath;
        }
        
        await organizationRepository.SaveAsync(cancellationToken);

        if (command.Logo is not null
            && !string.IsNullOrWhiteSpace(oldLogoPath)
            && !string.Equals(oldLogoPath, DefaultValues.OrganizationLogo, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(oldLogoPath, entity.LogoPath, StringComparison.OrdinalIgnoreCase))
        {
            await imageStorage.DeleteAsync(oldLogoPath, cancellationToken);
        }
    }
}
