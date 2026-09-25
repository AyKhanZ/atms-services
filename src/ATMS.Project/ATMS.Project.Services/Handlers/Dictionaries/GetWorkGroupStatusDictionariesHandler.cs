using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkGroupStatusDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkGroupStatusDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkGroupStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkGroupStatusesAsync(cancellationToken);
    }
}
