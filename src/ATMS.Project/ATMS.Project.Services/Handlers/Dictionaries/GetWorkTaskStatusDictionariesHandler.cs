using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTaskStatusDictionariesHandler(IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetWorkTaskStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkTaskStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var result = await dictionariesRepository.GetWorkTaskStatusesAsync(cancellationToken);
        
        return result.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}
