using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkItemPriorityDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkItemPriorityDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkItemPriorityDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkItemPrioritiesAsync(cancellationToken);
    }
}
