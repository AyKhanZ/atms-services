using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetMaritalStatusDictionariesHandler(
    IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetMaritalStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetMaritalStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var maritalStatuses = await dictionariesRepository.GetMaritalStatusesAsync(cancellationToken);
        
        return maritalStatuses.Select(ms => ms.ToDictionaryModel(ms.Translations, language)).ToArray();
    }
}
