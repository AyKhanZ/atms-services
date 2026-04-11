using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetUserTypesDictionariesHandler(
    IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetUserTypesDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetUserTypesDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var userStatuses = await dictionariesRepository.GetUserTypesAsync(cancellationToken);
        
        return userStatuses.Select(us => us.ToDictionaryModel(us.Translations, language)).ToArray();
    }
}
