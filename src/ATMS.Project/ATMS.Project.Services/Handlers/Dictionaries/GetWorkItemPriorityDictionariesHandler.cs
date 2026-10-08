using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public sealed class GetWorkItemPriorityDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkItemPriorityDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkItemPriorityDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkItemPrioritiesAsync(cancellationToken);
    }
}
