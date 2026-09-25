using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTaskStatusDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkTaskStatusDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkTaskStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkTaskStatusesAsync(cancellationToken);
    }
}
