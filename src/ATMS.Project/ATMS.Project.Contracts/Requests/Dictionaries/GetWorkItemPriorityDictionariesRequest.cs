using ATMS.Application.Models;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Dictionaries;

public sealed class GetWorkItemPriorityDictionariesRequest : IRequest<DictionaryModel[]>;
