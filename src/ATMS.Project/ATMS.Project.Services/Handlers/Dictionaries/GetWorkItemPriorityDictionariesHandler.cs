using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkItemPriorityDictionariesHandler(IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetWorkItemPriorityDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkItemPriorityDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var result = await dictionariesRepository.GetWorkItemPrioritiesAsync(cancellationToken);
        
        return result.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}
