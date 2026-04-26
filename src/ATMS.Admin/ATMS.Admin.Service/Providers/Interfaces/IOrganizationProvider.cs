using ATMS.Admin.Contracts.Models.Organizations;

namespace ATMS.Admin.Service.Providers.Interfaces;

public interface IOrganizationProvider
{
    Task<OrganizationModel?> GetAsync(Guid id, CancellationToken cancellationToken);
}
