using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Search;

// page size, sort and cursor are checked for every keyset request; here an empty query is an error (no recent list)
public sealed class GetGlobalSearchPageValidator : AbstractValidator<GetGlobalSearchPageRequest>
{
    public GetGlobalSearchPageValidator()
    {
        RuleFor(request => request.ItemType)
            .Must(itemType => Enum.IsDefined((GlobalSearchItemTypeEnum)itemType))
            .WithMessage(_ => GlobalSearchMessages.ItemTypeUnsupported);

        RuleFor(request => request.Q)
            .Must(query =>
            {
                var search = query?.Trim();
                if (string.IsNullOrWhiteSpace(search) || search.Length > 100)
                {
                    return false;
                }

                var code = search.TrimStart('#').Trim();
                var byCode = code.Length > 0 && code.All(char.IsDigit);
                var byTitle = search.Length >= 3 && !search.StartsWith('#');
                return byCode || byTitle;
            })
            .WithMessage(_ => GlobalSearchMessages.QueryLength);
    }
}
