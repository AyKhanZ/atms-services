using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public sealed class GetProjectKindDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetProjectKindDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetProjectKindDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetProjectKindsAsync(cancellationToken);
    }
}
