using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Migration;

public class ApplyMigrationsCommand : IRequest<MigrationModel> { }
