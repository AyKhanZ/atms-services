using ATMS.Admin.Contracts.Models.Localization;
using ATMS.Admin.Contracts.Requests.Localization;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace ATMS.Admin.Service.Handlers.Localization;

public sealed class GetDefaultLanguageHandler(IOptions<LocalizationOptions> localizationOptions)
    : IRequestHandler<GetDefaultLanguageRequest, DefaultLanguageModel>
{
    public Task<DefaultLanguageModel> Handle(
        GetDefaultLanguageRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new DefaultLanguageModel
        {
            DefaultLanguage = localizationOptions.Value.DefaultLanguage
        });
    }
}
