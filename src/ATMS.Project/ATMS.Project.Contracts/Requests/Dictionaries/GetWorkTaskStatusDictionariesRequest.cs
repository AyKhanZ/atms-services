using ATMS.Application.Models;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Dictionaries;

public class GetWorkTaskStatusDictionariesRequest : IRequest<DictionaryModel[]>;
