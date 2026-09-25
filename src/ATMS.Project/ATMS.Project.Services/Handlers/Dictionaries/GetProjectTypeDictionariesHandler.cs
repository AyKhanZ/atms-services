using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetProjectTypeDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetProjectTypeDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetProjectTypeDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetProjectTypesAsync(cancellationToken);
    }
}
