using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetUserStatusDictionariesHandler(
    IDictionariesRepository dictionariesRepository)
    : IRequestHandler<GetUserStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetUserStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var userStatuses = await dictionariesRepository.GetUserStatusesAsync(cancellationToken);
        
        return userStatuses.Select(us => us.ToDictionaryModel(us.Translations, language)).ToArray();
    }
}
