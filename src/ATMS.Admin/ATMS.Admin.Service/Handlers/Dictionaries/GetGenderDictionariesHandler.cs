using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetGenderDictionariesHandler(IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetGenderDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetGenderDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var genders = await dictionariesRepository.GetGendersAsync(cancellationToken);
        
        return genders.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}
