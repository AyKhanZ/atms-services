using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetProjectTypeDictionariesHandler(IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetProjectTypeDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetProjectTypeDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var result = await dictionariesRepository.GetProjectTypesAsync(cancellationToken);
        
        return result.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}
