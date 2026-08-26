using ATMS.Project.Contracts.Models.Organizations;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Project.Contracts.Requests.Organizations;

[Access(PermissionEnum.OrganizationView)]
public class GetOrganizationRequest : IRequest<OrganizationModel>
{
    public required Guid Id { get; set; }
}
