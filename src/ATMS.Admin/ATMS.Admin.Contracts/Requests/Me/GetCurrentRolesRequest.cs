using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public sealed class GetCurrentRolesRequest : IRequest<DictionaryModel<Guid>[]>;
