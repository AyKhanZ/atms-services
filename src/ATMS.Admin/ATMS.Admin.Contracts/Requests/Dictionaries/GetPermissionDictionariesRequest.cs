using ATMS.Admin.Contracts.Models.Dictionaries;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public sealed class GetPermissionDictionariesRequest : IRequest<PermissionModel[]>;
