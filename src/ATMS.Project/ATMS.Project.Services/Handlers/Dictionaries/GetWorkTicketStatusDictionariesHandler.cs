using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Services.Dictionaries.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTicketStatusDictionariesHandler(IDictionaryCacheService dictionaries)
    : IRequestHandler<GetWorkTicketStatusDictionariesRequest, DictionaryModel[]>
{
    public Task<DictionaryModel[]> Handle(GetWorkTicketStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return dictionaries.GetWorkTicketStatusesAsync(cancellationToken);
    }
}
