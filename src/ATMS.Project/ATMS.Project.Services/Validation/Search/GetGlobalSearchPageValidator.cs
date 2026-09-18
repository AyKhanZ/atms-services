using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Search;

/// <summary>
/// Page size, sort direction and cursor are already checked for every keyset request, so only
/// what belongs to this endpoint is here. Unlike the palette, an empty query means nothing on a
/// page of results: there is no recent list to fall back to.
/// </summary>
public class GetGlobalSearchPageValidator : AbstractValidator<GetGlobalSearchPageRequest>
{
    public GetGlobalSearchPageValidator()
    {
        RuleFor(request => request.ItemType)
            .IsInEnum()
            .WithMessage(_ => GlobalSearchMessages.ItemTypeUnsupported);

        RuleFor(request => request.Q)
            .Must(query =>
            {
                var search = query?.Trim();
                if (string.IsNullOrEmpty(search) || search.Length > 100)
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
