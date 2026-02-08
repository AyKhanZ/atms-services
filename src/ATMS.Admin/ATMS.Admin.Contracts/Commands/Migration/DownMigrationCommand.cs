using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Migration;

public class DownMigrationCommand : IRequest<MigrationModel>
{
    public string MigrationName { get; set; }
}
