using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTicketStatusDictionariesHandler(IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetWorkTicketStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkTicketStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var result = await dictionariesRepository.GetWorkTicketStatusesAsync(cancellationToken);
        
        return result.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}
