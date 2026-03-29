using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public class GetMaritalStatusDictionariesRequest : IRequest<DictionaryModel[]>;
