using ATMS.Application.Exceptions.Resources;
using ATMS.Contracts.Requests;
using FluentValidation;

namespace ATMS.Application.Dispatcher.Validation;

public abstract class PagedRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : GetPaginationRequest
{
    protected PagedRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ValidationMessages.PageMustBePositive);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage(ValidationMessages.PageSizeOutOfRange);

        RuleFor(request => request.SortDirection)
            .IsInEnum()
            .WithMessage(ValidationMessages.InvalidSortDirection);
    }
}

public sealed class PagedRequestValidator : PagedRequestValidator<GetPaginationRequest>;
