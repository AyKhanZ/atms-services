using ATMS.Project.Contracts.Requests.Search;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Search;

public class GetGlobalSearchValidator : AbstractValidator<GetGlobalSearchRequest>
{
    public GetGlobalSearchValidator()
    {
        RuleFor(request => request.Take)
            .InclusiveBetween(1, 50)
            .WithMessage(_ => GlobalSearchMessages.TakeOutOfRange);

        RuleFor(request => request.Q)
            .Must(query =>
            {
                var search = query?.Trim();
                if (string.IsNullOrEmpty(search))
                {
                    return true;
                }

                var code = search.TrimStart('#').Trim();
                var byCode = code.Length > 0 && code.All(char.IsDigit);
                var byTitle = search.Length >= 3 && !search.StartsWith('#');
                return search.Length <= 100 && (byCode || byTitle);
            })
            .WithMessage(_ => GlobalSearchMessages.QueryLength);
    }
}
