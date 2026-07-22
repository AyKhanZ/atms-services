using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public sealed class GetLanguageDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    IMapper mapper)
    : IRequestHandler<GetLanguageDictionariesRequest, LanguageModel[]>
{
    public async Task<LanguageModel[]> Handle(GetLanguageDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        var languages = await dictionariesRepository.GetLanguagesAsync(cancellationToken);

        return mapper.Map<LanguageModel[]>(languages);
    }
}
