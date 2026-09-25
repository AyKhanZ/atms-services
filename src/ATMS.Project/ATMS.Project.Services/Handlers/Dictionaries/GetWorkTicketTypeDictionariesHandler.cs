using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTicketTypeDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkTicketTypeDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkTicketTypeDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkTicketTypesAsync(cancellationToken);
    }
}
