using ATMS.Admin.Contracts.Models;
using MediatR;
using ATMS.Application.Security;

namespace ATMS.Admin.Contracts.Commands.Migration;

[SuperAdminAccess]
public class ApplyMigrationsCommand : IRequest<MigrationModel> { }
