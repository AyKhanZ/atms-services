using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public class GetUserTypesDictionariesRequest : IRequest<DictionaryModel[]>;
