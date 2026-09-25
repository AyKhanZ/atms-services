using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetProjectStatusDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetProjectStatusDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetProjectStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetProjectStatusesAsync(cancellationToken);
    }
}
