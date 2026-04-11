using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetPermissionDictionariesHandler(
    IPermissionRepository permissionRepository)
    : IRequestHandler<GetPermissionDictionariesRequest, PermissionModel[]>
{
    public async Task<PermissionModel[]> Handle(GetPermissionDictionariesRequest request, CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;
        var permissions = await permissionRepository.GetAsync(cancellationToken);
        
        return permissions.Select(p => new PermissionModel
        {
            Id = p.Id,
            Code = p.Code,
            Module = p.Module,
            Name = p.Translations.Resolve(language, p.Code)
        }).ToArray();
    }
}
