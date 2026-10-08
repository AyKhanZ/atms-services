using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public sealed class GetMaritalStatusDictionariesRequest : IRequest<DictionaryModel[]>;
