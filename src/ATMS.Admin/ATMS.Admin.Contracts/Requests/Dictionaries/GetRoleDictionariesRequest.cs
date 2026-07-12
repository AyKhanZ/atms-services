using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public class GetRoleDictionariesRequest : IRequest<DictionaryModel<Guid>[]>;