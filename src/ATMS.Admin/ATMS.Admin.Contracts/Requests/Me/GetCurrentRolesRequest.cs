using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public class GetCurrentRolesRequest : IRequest<DictionaryModel<Guid>[]>;
