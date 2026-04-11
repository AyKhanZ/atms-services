using ATMS.Application.Models;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Dictionaries;

public class GetWorkItemPriorityDictionariesRequest : IRequest<DictionaryModel[]>;
