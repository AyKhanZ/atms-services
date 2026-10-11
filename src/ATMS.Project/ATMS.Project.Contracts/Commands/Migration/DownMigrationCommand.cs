using ATMS.Application.Security;
using ATMS.Project.Contracts.Models;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Migration;

[SuperAdminAccess]
public sealed class DownMigrationCommand : IRequest<MigrationModel>
{
    public string MigrationName { get; set; }
}
